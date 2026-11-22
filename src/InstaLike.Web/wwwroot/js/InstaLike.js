$.ajaxSetup({
    headers: { 'RequestVerificationToken': $('meta[name="request-verification-token"]').attr('content') }
});

// Highlights hashtags (#string) and Users' nicknames (@string)
// Nicknames becomes clickable, redirecting the user to the related profile page
function highlightHashtagsAndNicknames() {
    const nicknamePattern = new RegExp('@\\w+', 'gi');
    const hashtagPattern = new RegExp('#\\w+', 'gi');

    $('.post-author-comment, .post-comments > li > p')
        .not('[data-highlighted]')
        .attr('data-highlighted', '')
        .each(function () {
            var commentText = this.innerHTML;

            commentText = commentText.replace(nicknamePattern, function (nickname) {
                return `<a class='at-nickname' href='/Member/Profile/${nickname.substring(1, nickname.length)}'>${nickname}</a>`;
            });

            this.innerHTML = commentText;
        })
        .each(function () {
            var commentText = this.innerHTML;

            commentText = commentText.replace(hashtagPattern, function (hashtag) {
                return `<span class='hashtag'>${hashtag}</span>`;
            });

            this.innerHTML = commentText;
        });
}

// Shows the dates in the viewer's time zone
function showDatesInLocalTime() {
    const weekday = new Intl.DateTimeFormat(document.documentElement.lang || undefined, { weekday: 'long' });
    const twoDigits = number => String(number).padStart(2, '0');
    const day = date => `${twoDigits(date.getDate())}/${twoDigits(date.getMonth() + 1)}/${date.getFullYear()}`;

    $('time[datetime]').each(function () {
        const date = new Date(this.getAttribute('datetime'));
        if (isNaN(date)) {
            return;
        }

        this.title = `${day(date)} ${twoDigits(date.getHours())}:${twoDigits(date.getMinutes())}`;
        if (this.dataset.format === 'day') {
            this.textContent = `${weekday.format(date)} ${day(date)}`;
        }
    });
}

// On load
$(function () {
    highlightHashtagsAndNicknames();
    showDatesInLocalTime();
});

// Displays user's profile picture after choosing it
$(".user-details-profile-picture input").change(function (e) {
    if (e.target.files && e.target.files[0]) {
        const reader = new FileReader();

        reader.onload = function (e) {
            $('.user-details-profile-picture > label > img').attr('src', e.target.result);
        };

        reader.readAsDataURL(e.target.files[0]);
    }
});

// Displays picture to share after choosing it
$(".form_post .picture-placeholder .posted-picture > input").change(function (e) {
    if (e.target.files && e.target.files[0]) {
        const reader = new FileReader();

        reader.onload = function (e) {
            $('.form_post .picture-placeholder .posted-picture > label > img').attr('src', e.target.result);
            // Remove tagging error message, if present
            $('.form_post .tags-container').find('span').remove();
        };

        reader.readAsDataURL(e.target.files[0]);
    }
});

// Adds a comment to a post
$(document).on('submit', '.form_comment', function (e) {
    e.preventDefault();

    var form = $(this),
        postId = form.find('#PostID').val(),
        comment = form.find('#comment'),
        postComments = form.parent().find('.post-comments'),
        showAllComments = form.parent().find('.show-all-comments'),
        authorComment = form.parent().find('.post-author-comment'),
        commentText = comment.val();

    if (!commentText) {
        return false;
    }

    $.post("/Post/PublishComment", { PostID: postId, CommentText: commentText }, function (response) {
        postComments.remove();
        showAllComments.remove();

        // Updates markup
        $(response).insertAfter(authorComment);

        // Cleanup comment text box
        comment.val('');

        highlightHashtagsAndNicknames();
        showDatesInLocalTime();
    });
});

// Shows or hides the comments after the first five
$(document).on('click', '.show-all-comments', function () {
    const button = $(this);
    const expanded = button.attr('aria-expanded') !== 'true';

    button.siblings('.post-comments').find('li.comment-hidden').toggleClass('show-all', expanded);
    button.attr('aria-expanded', expanded);
    button.text(button.data(expanded ? 'hide-label' : 'show-label'));
});

// Loads older posts when the end of the timeline comes into view.
// The server renders the .older-posts line only when there are older posts, with the address of the next ones.
const olderPostsObserver = new IntersectionObserver(function (entries) {
    entries.filter(entry => entry.isIntersecting).forEach(function (entry) {
        const olderPosts = entry.target;
        olderPostsObserver.unobserve(olderPosts);
        olderPosts.textContent = 'Loading older posts…';

        $.get(olderPosts.dataset.url, function (response) {
            $(olderPosts).replaceWith(response);
            highlightHashtagsAndNicknames();
            showDatesInLocalTime();
            $('.older-posts').each(function () {
                olderPostsObserver.observe(this);
            });
        }).fail(function () {
            olderPosts.textContent = 'The older posts could not be loaded. Reload the page to try again.';
        });
    });
}, { rootMargin: '0px 0px 600px 0px' });

$('.older-posts').each(function () {
    olderPostsObserver.observe(this);
});

// Sets o removes a "Like" on a post.
$(document).on('click', '.like-button', function (e) {
    e.preventDefault();

    var likeButton = $(this),
        likesMarkup = likeButton.next('strong').find('.likes-count'),
        likes = parseInt(likesMarkup.html()),
        postId = likeButton.attr('data-post-id');

    var endpoint = likeButton.hasClass('post-liked') ? '/Post/Dislike' : '/Post/Like';

    likeButton.siblings('.like-error').remove();

    $.post(endpoint, { PostID: postId }, function () {
        likeButton.toggleClass('post-liked');

        // Increments o decrements the number of likes.
        const increment = likeButton.hasClass('post-liked') ? 1 : -1;
        likesMarkup.html(likes + increment);
    }).fail(function (response) {
        $('<span class="like-error"></span>').text(response.responseText).insertAfter(likeButton.next('strong'));
    });
});

// Show popup
$('.popup-link').click(function (e) {
    e.preventDefault();

    var popup = $('.popup'),
        popupContent = $('.popup-content');

    var link = $(this),
        url = link.attr('data-ajax-url');

    $.get(url, function (response) {
        popupContent.empty().append(response);
        popup.addClass('popup-activated');
    });
});

// Close popup
$('.popup-close').click(function (e) {
    e.preventDefault();

    if ($('.popup').hasClass('popup-activated')) {
        $('.popup').removeClass('popup-activated');
        $('.popup-content').empty();
    }
});

// Pictures auto tagging: the chosen picture, or else the one of the post being edited
$('.form_post .autotag-button').click(function (e) {
    e.preventDefault();

    const formData = new FormData();
    const chosenPicture = $('.posted-picture > input')[0].files[0];
    const editedPostID = $('.form_post input[name="PostID"]').val();
    if (chosenPicture) {
        formData.append('PictureFile', chosenPicture);
    } else if (editedPostID) {
        formData.append('PostID', editedPostID);
    } else {
        return;
    }

    suggestTags(formData);
});

// Add or remove a suggested tag from the post text.
// Delegated to the list, so tags added by later suggestions get a single handler too.
$('.form_post .tags').on('click', '.tag', function (e) {
    e.preventDefault();

    const selectedTag = $(this);
    const tag = selectedTag.text();
    const textInput = $('.form_post textarea');
    const currentText = textInput.val().trim();

    selectedTag.toggleClass('tag-selected');
    if (selectedTag.hasClass('tag-selected')) {
        textInput.val(currentText.length > 0 ? `${currentText} ${tag}` : tag);
    } else {
        textInput.val(withoutTag(currentText, tag));
    }
});

// Removes the tag only as a whole word, together with the space before it: '#cat' leaves '#category' alone.
function withoutTag(text, tag) {
    const escapedTag = tag.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    return text.replace(new RegExp(`(^|\\s+)${escapedTag}(?=\\s|$)`), '').trim();
}

function suggestTags(formData) {
    var tagList = $('.form_post ul');
    $.ajax({
        type: 'POST',
        url: '/Post/Autotag',
        data: formData,
        contentType: false,
        processData: false,
        cache: false,
        beforeSend: function () {
            tagList.empty();
            $('.form_post .tags-container > span').remove();
            $('.form_post .autotag-button').addClass('button-progress').prop('disabled', true);
        },
        success: function (response) {
            response.forEach(function (tag) {
                const tagLink = $('<a href="#" class="tag"></a>').text(tag);
                tagList.append($('<li></li>').append(tagLink));
            });
        },
        error: function (response) {
            // Reports tagging error
            $('.form_post .tags-container').append($('<span></span>').text(`Tagging error: ${response.responseText}`));
        },
        complete: function () {
            $('.form_post .autotag-button').removeClass('button-progress').prop('disabled', false);
        }
    });
}