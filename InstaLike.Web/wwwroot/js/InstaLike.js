// Highlights hashtags (#string) and Users' nicknames (@string)
function highlightHashtagsAndNicknames() {
    const NicknamePattern = new RegExp('@\\w+', 'gi');
    const HashtagPattern = new RegExp('#\\w+', 'gi');

    $('.post-author-comment, .post-comments > li > p')
        .each(function () {
            var commentText = this.innerHTML;

            commentText = commentText.replace(NicknamePattern, function (nickname) {
                return `<a class='at-nickname' href='/Account/Profile/${nickname.substring(1, nickname.length)}'>${nickname}</a>`;
            });

            this.innerHTML = commentText;
        })
        .each(function () {
            var commentText = this.innerHTML;

            commentText = commentText.replace(HashtagPattern, function (hashtag) {
                return `<span class='hashtag'>${hashtag}</span>`;
            });

            this.innerHTML = commentText;
        });
}

$(function () {
    highlightHashtagsAndNicknames();
});

// Displays user's profile picture after choosing it
$(".user-details-profile-picture input").change(function (e) {
    if (e.target.files && e.target.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('.user-details-profile-picture > label > img').attr('src', e.target.result);
        };

        reader.readAsDataURL(e.target.files[0]);
    }
});

// Adds a comment to a post
$('.form_comment').submit(function (e) {
    e.preventDefault();

    var form = $(this),
        postID = form.find('#PostID').val(),
        comment = form.find('#comment'),
        postComments = form.parent().find('.post-comments'),
        showAllComments = form.parent().find('.show-all-comments'),
        authorComment = form.parent().find('.post-author-comment'),
        commentText = comment.val();

    if (!commentText) {
        return false;
    }

    $.post("/Post/PublishComment", { PostID: postID, CommentText: commentText }, function (response) {
        postComments.remove();
        showAllComments.remove();

        // Updates markup
        $(response).insertAfter(authorComment);

        // Cleanup comment textbox
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
        postID = likeButton.attr('data-post-id'),
        userID = likeButton.attr('data-user-id');

    var endpoint = likeButton.hasClass('post-liked') ? '/Post/Dislike' : '/Post/Like';

    $.post(endpoint, { PostID: postID, UserID: userID }, function () {
        likeButton.toggleClass('post-liked');

        // Increments o decrements the number of likes.
        var increment = likeButton.hasClass('post-liked') ? 1 : -1;
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
    if ($('.popup').hasClass('popup-activated')) {
        $('.popup').removeClass('popup-activated');
        $('.popup-content').empty();
    }
});