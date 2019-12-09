var app = angular.module("myApp", []);


app.controller("myCtrl", function ($scope, $rootScope) {
    $scope.name = "hello d";
    $rootScope.rootName = "rootName";

});

app.controller("namsController", function ($scope) {
    $scope.names = [
        { name: 'name11', age: '60' },
        { name: 'name225', age: '50' },
        { name: 'name22', age: '20' },
        { name: 'name33', age: '30' }
    ];
});

app.controller("serviceController", function ($scope, $location, $http) {
    $scope.url = $location.absUrl();
    $scope.GetHttp = function () {
        $http.get("https://www.c-sharpcorner.com/article/angularjs-crud-operation-using-asp-net-mvc/").then(function (response) {
            alert(response);
        }, function (res) {
            alert(res);
        })
    }
});

app.controller("httpController", function ($scope, $http) {
    $scope.GetHttp = function () {
        alert('adfad');
        $http.get("http://localhost:53479/Views/index.html").then(function (response) {
            $scope.htmlStr = response.data.sites;
        }, function (res) {
            alert(res.data);
        });

    }
});

app.controller("timeoutController", function ($scope, $timeout) {
    $scope.timeoutStr = "1秒之后会出现。。。";
    $timeout(function () {
        $scope.timeoutStr = "你好，，，";
    }, 1000)
});

app.controller("intervalController", function ($scope, $interval) {
    $scope.timerStr = new Date().toLocaleDateString();
    $interval(function () {
        $scope.timerStr = new Date().toLocaleDateString();
    }, 1000);
})


app.service("customService", function () {
    this.myFunc = function (m) {
        m = m + '====';
        return m;
    }
});

app.controller("cserviceController", function ($scope, customService) {
    $scope.hex = customService.myFunc('abcd');
})

app.controller("selectController", function ($scope) {
    $scope.names = ["Google", "Runoob", "Taobao"];
    // 使用数组
    $scope.sites = [
        { site: "google", url: 'http://www.google.com' },
        { site: "taobao", url: 'http://www.taobao.com' },
    ];
    // 使用对象
    $scope.sitesobject = {
        site01: 'google',
        site02: 'taobao',
    }
    // value 为一个对象
    $scope.cars = {
        car01: { name: 'Byd', price: '6w' },
        car02: { name: '现代', price: '10w' },
        car03: { name: '宝骏', price: '20w' },
    }
})

app.controller("htmlDomController", function ($scope) {
    $scope.isChecked = false;
    $scope.isShow = false;
    $scope.toggle = function () {
        $scope.isShow = !$scope.isShow;

    };
});

app.controller("formCtrl", function ($scope) {
    $scope.master = { name: 'zhangsan', age: '10' };

    $scope.Reset = function () {
        $scope.user = angular.copy($scope.master);
    }
    $scope.Reset();
    $scope.x1 = "ABCD";
    $scope.lowerStr = angular.$$lowercase($scope.x1);
    $scope.x2 = angular.$$uppercase($scope.lowerStr);
});


// 创建value对象
app.value("defaultValue", 100);
// 创建函数，用于提供返回值
app.factory('mathService', function () {
    var factory = {};
    factory.multiply = function (a, b) {
        return a * b;
    };
    return factory;
});
app.service('calcService', function (mathService) {
    this.square = function (a, b) {
        return mathService.multiply(a, b);
    }
});


app.controller("dICtrl", function ($scope, defaultValue, calcService) {
    $scope.number1 = defaultValue;
    $scope.a = 10;
    $scope.b = 20;
    
    $scope.calc = function () {
        $scope.c = calcService.square($scope.a, $scope.b);
    }
})
