// Adds a comment to a post
$('.form-comment').submit(function (e) {
    e.preventDefault();

    var form = $(this),
        postID = form.find('#PostID').val(),
        comment = form.find('#comment'),
        postComments = form.parent().find('.commenti-post'),
        showAllComments = form.parent().find('.visualizza-tutti-commenti'),
        authorComment = form.parent().find('.commento-autore-post'),
        commentText = comment.val();

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
$('.visualizza-tutti-commenti').click(function (e) {
    e.preventDefault();

    $(this).siblings('.commenti-post').find('li.nascosto').toggleClass('mostra-tutti');
});

// Sets o removes a "Like" to a post.
$('.pulsante-like').click(function (e) {
    e.preventDefault();

    var pulsante = $(this),
        markupNumeroLike = pulsante.next('strong').find('.numero-like'),
        numeroLikeRicevuti = parseInt(markupNumeroLike.html()),
        idPost = pulsante.attr('data-post-id');

    $.post('/Post/Like', { IDPost: idPost }, function (response) {
        pulsante.toggleClass('post-piaciuto');

        // Increments o decrements the number of likes.
        var incremento = pulsante.hasClass('post-piaciuto') ? 1 : -1;
        markupNumeroLike.html(numeroLikeRicevuti + incremento);
    });
});

// Show popup
$('.popup-link').click(function (e) {
    e.preventDefault();

    var popup = $('.popup'),
        contenutoPopup = $('.popup-contenuto');

    var link = $(this),
        url = link.attr('data-ajax-url');

    $.get(url, function (response) {
        contenutoPopup.empty().append(response);
        popup.addClass('popup-attivo');
    });
});

// Close popup
$('.popup-chiudi').click(function (e) {
    if ($('.popup').hasClass('popup-attivo')) {
        $('.popup').removeClass('popup-attivo');
        $('.popup-contenuto').empty();
    }
});