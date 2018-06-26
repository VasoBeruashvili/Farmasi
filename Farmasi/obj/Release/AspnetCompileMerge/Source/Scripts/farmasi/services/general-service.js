app.service('generalService', function ($http, $window) {
    this.addToCart = function (productId, quantity) {
        $http.post('/shopping/addToCart', { productId: productId, quantity: quantity }).then(function (resp) {
            if (resp.data) {
                $window.location.reload();
            } else {
                alert("*კალათში დამატება ვერ მოხერხდა!\n*საწყობში არ არის მითითებული რაოდენობის საქონელი!");
            }
        });
    }

    this.removeFromCart = function (productId, quantity) {
        $http.post('/shopping/removeFromCart', { productId: productId, quantity: quantity }).then(function (resp) {
            if (resp.data) {
                $window.location.reload();
            }
        });
    }
});

app.filter('myTableFilter', function () {
    return function (dataArray, searchTerm) {
        if (!dataArray) {
            return;
        }
        else if (!searchTerm) {
            return dataArray;
        }
        else {
            var term = searchTerm.toLowerCase();
            return dataArray.filter(function (item) {
                var termInCode = item.Code.toLowerCase().indexOf(term) > -1;
                var termInName = item.Name.toLowerCase().indexOf(term) > -1;
                return termInCode || termInName;
            });
        }
    }
});

app.service('pickerService', function () {
    var date_from, date_to;

    var setDates = function (new_date_from, new_date_to) {
        date_from = new_date_from;
        date_to = new_date_to;
    }

    var getDates = function () {
        return { date_from: date_from, date_to: date_to };
    }

    var daysInMonth = function (m, y) {
        return 32 - moment(y, m, 32);
    }

    var getFromDateString = function () {
        return moment(date_from).format('YYYY-MM-DD') + ' 00:00:00';
    }

    var getToDateString = function () {
        return moment(date_to).format('YYYY-MM-DD') + ' 23:59:59';
    }

    return {
        setDates: setDates,
        getDates: getDates,
        daysInMonth: daysInMonth,
        getFromDateString: getFromDateString,
        getToDateString: getToDateString
    };
});