var app = angular.module('farmasi-app', ['ui.bootstrap']);

function openProductDetails(productId) {
    window.location.href = '/product/details/' + productId;
}