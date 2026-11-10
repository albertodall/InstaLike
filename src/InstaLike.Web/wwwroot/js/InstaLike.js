// Highlights hashtags (#string) and Users' nicknames (@string)
// Nicknames becomes clickable, redirecting the user to the related profile page
function highlightHashtagsAndNicknames() {
    const nicknamePattern = new RegExp('@\\w+', 'gi');
    const hashtagPattern = new RegExp('#\\w+', 'gi');

    $('.post-author-comment, .post-comments > li > p')
        .each(function () {
            var commentText = this.innerHTML;

            commentText = commentText.replace(nicknamePattern, function (nickname) {
                return `<a class='at-nickname' href='/Account/Profile/${nickname.substring(1, nickname.length)}'>${nickname}</a>`;
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

// On load
$(function () {
    highlightHashtagsAndNicknames();
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
$('.form_comment').submit(function (e) {
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
    });
});

// Shows all comments
$('.show-all-comments').click(function (e) {
    e.preventDefault();

    $(this).siblings('.post-comments').find('li.hidden').toggleClass('show-all');
});

// Sets o removes a "Like" on a post.
$('.like-button').click(function (e) {
    e.preventDefault();

    var likeButton = $(this),
        likesMarkup = likeButton.next('strong').find('.likes-count'),
        likes = parseInt(likesMarkup.html()),
        postId = likeButton.attr('data-post-id'),
        userId = likeButton.attr('data-user-id');

    var endpoint = likeButton.hasClass('post-liked') ? '/Post/Dislike' : '/Post/Like';

    $.post(endpoint, { PostID: postId, UserID: userId }, function () {
        likeButton.toggleClass('post-liked');

        // Increments o decrements the number of likes.
        const increment = likeButton.hasClass('post-liked') ? 1 : -1;
        likesMarkup.html(likes + increment);
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

// Pictures auto tagging
$('.form_post .autotag-button').click(function (e) {
    e.preventDefault();

    var formData = new FormData();
    var pictureFileSelector = $(".posted-picture > input")[0];
    if (pictureFileSelector.files && pictureFileSelector.files[0]) {
        formData.append(pictureFileSelector.files[0].name, pictureFileSelector.files[0]);

        var tagList = $('.form_post ul');
        $.ajax({
            type: 'POST',
            url: '/Post/Autotag',
            data: formData,
            contentType: false,
            processData: false,
            cache: false,
            beforeSend: function () {
                $('.form_post .autotag-button').addClass('button-progress');
            },
            success: function (response) {
                response.forEach(function (tag) {
                    tagList.append(`<li><a href="#" class="tag">${tag}</a></li>`);
                });

                // Add or remove a tag from a picture comment
                $('.form_post .tag').click(function (e) {
                    e.preventDefault();

                    var tagSeparator = '';
                    const selectedTag = $(e.target);
                    const textInput = $('.form_post > div > input[type="text"]');
                    const currentText = textInput.val().trim();

                    selectedTag.toggleClass('tag-selected');
                    if (selectedTag.hasClass('tag-selected')) {
                        if (currentText.length > 0) {
                            // If there's any text, adds a trailing space to the tag.
                            tagSeparator = ' ';
                        }
                        textInput.val(currentText.concat(tagSeparator, selectedTag.text()));
                    } else {
                        // Replaces tag even if it has a trailing space (separator)
                        textInput.val(currentText.replace(` ${selectedTag.text()}`, ''));
                        textInput.val(currentText.replace(selectedTag.text(), ''));
                    }
                });

                $('.form_post .autotag-button').removeClass('button-progress');
            },
            error: function (response) {
                // Reports tagging error
                $('.form_post .tags-container').append(`<span>Tagging error: ${response.responseText}</span>`);
                $('.form_post .autotag-button').removeClass('button-progress');
            }
        });
    }
});