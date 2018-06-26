app.controller('shoppingCart-ctrl', function ($scope, generalService, $http, $window) {
    $scope.products = products;

    $scope.openProductDetails = function (productId) {
        openProductDetails(productId);
    }

    $scope.removeFromCart = function (productId, quantity) {
        generalService.removeFromCart(productId, quantity);
    }

    $scope.placeOrder = function () {
        $http.get('/shopping/placeOrder').then(function (resp) {
            if (resp.data) {
                $window.location.href = '/home/index';
            }
        });
    }
});