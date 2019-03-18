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
    });
});

// Shows all comments
$('.show-all-comments').click(function (e) {
    e.preventDefault();

    $(this).siblings('.post-comments').find('li.hidden').toggleClass('show-all');
});

// Sets o removes a "Like" to a post.
$('.pulsante-like').click(function (e) {
    e.preventDefault();

    var likeButton = $(this),
        likesMarkup = likeButton.next('strong').find('.numero-like'),
        likes = parseInt(likesMarkup.html()),
        postID = likeButton.attr('data-post-id'),
        userID = likeButton.attr('data-user-id');

    $.post('/Post/Like', { PostID: postID, UserID: userID }, function () {
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
        contenutoPopup = $('.popup-content');

    var link = $(this),
        url = link.attr('data-ajax-url');

    $.get(url, function (response) {
        contenutoPopup.empty().append(response);
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