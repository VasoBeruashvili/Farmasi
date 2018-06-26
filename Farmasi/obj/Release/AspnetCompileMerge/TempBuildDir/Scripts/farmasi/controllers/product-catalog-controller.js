app.controller('productCatalog-ctrl', function ($scope, $http, generalService) {
    $scope.products = products;

    $scope.openProductDetails = function (productId) {
        openProductDetails(productId);
    }

    $scope.addToCart = function (productId, quantity) {
        generalService.addToCart(productId, quantity);
    }

    $scope.searchCatalogProducts = function (code) {
        $http.post('/product/searchCatalogProducts', { code: code }).then(function (resp) {
            $scope.products = resp.data;
        });
    }
});