app.controller('productDetails-ctrl', function ($scope, $http, generalService) {
    $scope.addToCart = function (productId, quantity) {
        generalService.addToCart(productId, quantity);
    }
});