// Adds a comment to a post
$('.form-commento').submit(function (e) {
    e.preventDefault();

    var form = $(this),
        idPost = form.find('#IDPost').val(),
        commento = form.find('#commento'),
        commentiPost = form.parent().find('.commenti-post'),
        visualizzaTuttiCommenti = form.parent().find('.visualizza-tutti-commenti'),
        commentoAutorePost = form.parent().find('.commento-autore-post'),
        testoCommento = commento.val();

    $.post("/Post/AggiungiCommento", { IDPost: idPost, commento: testoCommento }, function (response) {
        commentiPost.remove();
        visualizzaTuttiCommenti.remove();

        // Updates markup
        $(response).insertAfter(commentoAutorePost);

        // Cleanup comment textbox
        commento.val('');
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