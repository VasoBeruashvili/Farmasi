$(document).ready(function(){
    $('.newly-added-products').slick({
        infinite: true,
        slidesToShow: 5,
        slidesToScroll: 5,
        accessibility: true,
        autoplay: true,
        autoplaySpeed: 4000,
        dots: true,
        speed: 1600
    });
});