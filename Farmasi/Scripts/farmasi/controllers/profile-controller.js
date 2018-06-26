app.controller('profile-ctrl', function ($scope, generalService, $http, $window, pickerService) {
    selectRfLink = function () {
        var el = document.getElementById("rf_link");
        var range = document.createRange();
        range.selectNodeContents(el);
        var sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(range);
    }

    var dt = moment();

    $scope.openFromDatepicker = function ($event) {
        $scope.from_opened = true;
    };

    $scope.openToDatepicker = function ($event) {
        $scope.to_opened = true;
    };

    $scope.dateOptions = {
        startingDay: 1,
        showWeeks: false
    };

    $scope.menuChoiceClick = function (sign) {
        var year = dt.year();
        switch (sign) {
            case "today":
                $scope.date_from = filterDate(moment());
                $scope.date_to = filterDate(moment());
                break;
            case "year":
                $scope.date_from = filterDate(moment([year, 0, 1]));
                $scope.date_to = filterDate(moment([year, 11, 31]));
                break;
            case "kvartali1":
                $scope.date_from = filterDate(moment([year, 0, 1]));
                $scope.date_to = filterDate(moment([year, 2, 31]));
                break;
            case "kvartali2":
                $scope.date_from = filterDate(moment([year, 3, 1]));
                $scope.date_to = filterDate(moment([year, 5, 30]));
                break;
            case "kvartali3":
                $scope.date_from = filterDate(moment([year, 6, 1]));
                $scope.date_to = filterDate(moment([year, 8, 30]));
                break;
            case "kvartali4":
                $scope.date_from = filterDate(moment([year, 9, 1]));
                $scope.date_to = filterDate(moment([year, 11, 31]));
                break;
            case "yan":
                $scope.date_from = filterDate(moment([year, 0, 1]));
                $scope.date_to = filterDate(moment([year, 0, 31]));
                break;
            case "feb":
                $scope.date_from = filterDate(moment([year, 1, 1]));
                $scope.date_to = filterDate(moment([year, 1, moment([year, 1]).daysInMonth()]));
                break;
            case "mar":
                $scope.date_from = filterDate(moment([year, 2, 1]));
                $scope.date_to = filterDate(moment([year, 2, 31]));
                break;
            case "apr":
                $scope.date_from = filterDate(moment([year, 3, 1]));
                $scope.date_to = filterDate(moment([year, 3, 30]));
                break;
            case "may":
                $scope.date_from = filterDate(moment([year, 4, 1]));
                $scope.date_to = filterDate(moment([year, 4, 31]));
                break
            case "jun":
                $scope.date_from = filterDate(moment([year, 5, 1]));
                $scope.date_to = filterDate(moment([year, 5, 30]));
                break;
            case "jul":
                $scope.date_from = filterDate(moment([year, 6, 1]));
                $scope.date_to = filterDate(moment([year, 6, 31]));
                break;
            case "aug":
                $scope.date_from = filterDate(moment([year, 7, 1]));
                $scope.date_to = filterDate(moment([year, 7, 31]));
                break;
            case "sep":
                $scope.date_from = filterDate(moment([year, 8, 1]));
                $scope.date_to = filterDate(moment([year, 8, 30]));
                break;
            case "oct":
                $scope.date_from = filterDate(moment([year, 9, 1]));
                $scope.date_to = filterDate(moment([year, 9, 31]));
                break;
            case "nov":
                $scope.date_from = filterDate(moment([year, 10, 1]));
                $scope.date_to = filterDate(moment([year, 10, 30]));
                break;
            case "dec":
                $scope.date_from = filterDate(moment([year, 11, 1]));
                $scope.date_to = filterDate(moment([year, 11, 31]));
                break;
        }

        pickerService.setDates($scope.date_from, $scope.date_to);
    };

    var filterDate = function (dat) {
        return dat.toDate();
    }

    $scope.date_from = filterDate(dt);
    $scope.date_to = filterDate(dt);

    $scope.cid = null;

    $scope.refresh = function () {
        $http.post('/product/getProductOuts', { fromDate: moment($scope.date_from).format('YYYY-MM-DD'), toDate: moment($scope.date_to).format('YYYY-MM-DD'), cid: $scope.cid }).then(function (resp) {
            $scope.productOuts = resp.data;
            angular.forEach($scope.productOuts, function (po) {
                po.tdate = moment(po.tdate).format('DD.MM.YYYY HH:mm:ss');
            });
        });
    }

    getContragentInfoById = function (cid) {
        $scope.cid = cid;
        $scope.refresh();
    }

    $scope.refresh();


    $scope.date_from1 = filterDate(dt);
    $scope.date_to1 = filterDate(dt);

    $scope.dateOptions1 = {
        startingDay: 1,
        showWeeks: false
    };

    $scope.openFromDatepicker1 = function ($event) {
        $scope.from_opened1 = true;
    };

    $scope.openToDatepicker1 = function ($event) {
        $scope.to_opened1 = true;
    };

    $scope.menuChoiceClick1 = function (sign) {
        var year = dt.year();
        switch (sign) {
            case "today":
                $scope.date_from1 = filterDate(moment());
                $scope.date_to1 = filterDate(moment());
                break;
            case "year":
                $scope.date_from1 = filterDate(moment([year, 0, 1]));
                $scope.date_to1 = filterDate(moment([year, 11, 31]));
                break;
            case "kvartali1":
                $scope.date_from1 = filterDate(moment([year, 0, 1]));
                $scope.date_to1 = filterDate(moment([year, 2, 31]));
                break;
            case "kvartali2":
                $scope.date_from1 = filterDate(moment([year, 3, 1]));
                $scope.date_to1 = filterDate(moment([year, 5, 30]));
                break;
            case "kvartali3":
                $scope.date_from1 = filterDate(moment([year, 6, 1]));
                $scope.date_to1 = filterDate(moment([year, 8, 30]));
                break;
            case "kvartali4":
                $scope.date_from1 = filterDate(moment([year, 9, 1]));
                $scope.date_to1 = filterDate(moment([year, 11, 31]));
                break;
            case "yan":
                $scope.date_from1 = filterDate(moment([year, 0, 1]));
                $scope.date_to1 = filterDate(moment([year, 0, 31]));
                break;
            case "feb":
                $scope.date_from1 = filterDate(moment([year, 1, 1]));
                $scope.date_to1 = filterDate(moment([year, 1, moment([year, 1]).daysInMonth()]));
                break;
            case "mar":
                $scope.date_from1 = filterDate(moment([year, 2, 1]));
                $scope.date_to1 = filterDate(moment([year, 2, 31]));
                break;
            case "apr":
                $scope.date_from1 = filterDate(moment([year, 3, 1]));
                $scope.date_to1 = filterDate(moment([year, 3, 30]));
                break;
            case "may":
                $scope.date_from1 = filterDate(moment([year, 4, 1]));
                $scope.date_to1 = filterDate(moment([year, 4, 31]));
                break
            case "jun":
                $scope.date_from1 = filterDate(moment([year, 5, 1]));
                $scope.date_to1 = filterDate(moment([year, 5, 30]));
                break;
            case "jul":
                $scope.date_from1 = filterDate(moment([year, 6, 1]));
                $scope.date_to1 = filterDate(moment([year, 6, 31]));
                break;
            case "aug":
                $scope.date_from1 = filterDate(moment([year, 7, 1]));
                $scope.date_to1 = filterDate(moment([year, 7, 31]));
                break;
            case "sep":
                $scope.date_from1 = filterDate(moment([year, 8, 1]));
                $scope.date_to1 = filterDate(moment([year, 8, 30]));
                break;
            case "oct":
                $scope.date_from1 = filterDate(moment([year, 9, 1]));
                $scope.date_to1 = filterDate(moment([year, 9, 31]));
                break;
            case "nov":
                $scope.date_from1 = filterDate(moment([year, 10, 1]));
                $scope.date_to1 = filterDate(moment([year, 10, 30]));
                break;
            case "dec":
                $scope.date_from1 = filterDate(moment([year, 11, 1]));
                $scope.date_to1 = filterDate(moment([year, 11, 31]));
                break;
        }

        pickerService.setDates($scope.date_from1, $scope.date_to1);
    };

    $scope.refresh1 = function () {
        $http.post('/product/getContragentRelation', { fromDate: moment($scope.date_from1).format('YYYY-MM-DD'), toDate: moment($scope.date_to1).format('YYYY-MM-DD') }).then(function (resp) {
            $scope.contRelation = resp.data;
        });
    }

    $scope.refresh1();



    $scope.date_from2 = filterDate(dt);
    $scope.date_to2 = filterDate(dt);

    $scope.dateOptions2 = {
        startingDay: 1,
        showWeeks: false
    };

    $scope.openFromDatepicker2 = function ($event) {
        $scope.from_opened2 = true;
    };

    $scope.openToDatepicker2 = function ($event) {
        $scope.to_opened2 = true;
    };

    $scope.menuChoiceClick2 = function (sign) {
        var year = dt.year();
        switch (sign) {
            case "today":
                $scope.date_from2 = filterDate(moment());
                $scope.date_to2 = filterDate(moment());
                break;
            case "year":
                $scope.date_from2 = filterDate(moment([year, 0, 1]));
                $scope.date_to2 = filterDate(moment([year, 11, 31]));
                break;
            case "kvartali1":
                $scope.date_from2 = filterDate(moment([year, 0, 1]));
                $scope.date_to2 = filterDate(moment([year, 2, 31]));
                break;
            case "kvartali2":
                $scope.date_from2 = filterDate(moment([year, 3, 1]));
                $scope.date_to2 = filterDate(moment([year, 5, 30]));
                break;
            case "kvartali3":
                $scope.date_from2 = filterDate(moment([year, 6, 1]));
                $scope.date_to2 = filterDate(moment([year, 8, 30]));
                break;
            case "kvartali4":
                $scope.date_from2 = filterDate(moment([year, 9, 1]));
                $scope.date_to2 = filterDate(moment([year, 11, 31]));
                break;
            case "yan":
                $scope.date_from2 = filterDate(moment([year, 0, 1]));
                $scope.date_to2 = filterDate(moment([year, 0, 31]));
                break;
            case "feb":
                $scope.date_from2 = filterDate(moment([year, 1, 1]));
                $scope.date_to2 = filterDate(moment([year, 1, moment([year, 1]).daysInMonth()]));
                break;
            case "mar":
                $scope.date_from2 = filterDate(moment([year, 2, 1]));
                $scope.date_to2 = filterDate(moment([year, 2, 31]));
                break;
            case "apr":
                $scope.date_from2 = filterDate(moment([year, 3, 1]));
                $scope.date_to2 = filterDate(moment([year, 3, 30]));
                break;
            case "may":
                $scope.date_from2 = filterDate(moment([year, 4, 1]));
                $scope.date_to2 = filterDate(moment([year, 4, 31]));
                break
            case "jun":
                $scope.date_from2 = filterDate(moment([year, 5, 1]));
                $scope.date_to2 = filterDate(moment([year, 5, 30]));
                break;
            case "jul":
                $scope.date_from2 = filterDate(moment([year, 6, 1]));
                $scope.date_to2 = filterDate(moment([year, 6, 31]));
                break;
            case "aug":
                $scope.date_from2 = filterDate(moment([year, 7, 1]));
                $scope.date_to2 = filterDate(moment([year, 7, 31]));
                break;
            case "sep":
                $scope.date_from2 = filterDate(moment([year, 8, 1]));
                $scope.date_to2 = filterDate(moment([year, 8, 30]));
                break;
            case "oct":
                $scope.date_from2 = filterDate(moment([year, 9, 1]));
                $scope.date_to2 = filterDate(moment([year, 9, 31]));
                break;
            case "nov":
                $scope.date_from2 = filterDate(moment([year, 10, 1]));
                $scope.date_to2 = filterDate(moment([year, 10, 30]));
                break;
            case "dec":
                $scope.date_from2 = filterDate(moment([year, 11, 1]));
                $scope.date_to2 = filterDate(moment([year, 11, 31]));
                break;
        }

        pickerService.setDates($scope.date_from2, $scope.date_to2);
    };

    $scope.refresh2 = function () {
        $http.post('/home/getRegisteredStaffs', { fromDate: moment($scope.date_from2).format('YYYY-MM-DD'), toDate: moment($scope.date_to2).format('YYYY-MM-DD') }).then(function (resp) {
            angular.forEach(resp.data, function (rs) {
                rs.createDate = moment(rs.createDate).format('DD.MM.YYYY HH:mm:ss');
            });
            $scope.registeredStaff = resp.data;
        });
    }

    $scope.generate = function () {
        $scope.showLoader = true;
        $http.get('/account/generate').then(function () {
            //$scope.showLoader = false;
            $window.location.reload();
        });
    }
});