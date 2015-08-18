/// <reference path='angular-signalr-hub.d.ts' />
/// <reference path='../angularjs/angular.d.ts' />
angular
    .module('app', ['SignalR'])
    .factory('Employees', ngSignalrTest.EmployeesFactory);
var ngSignalrTest;
(function (ngSignalrTest) {
    var EmployeesFactory = (function () {
        function EmployeesFactory($rootScope, Hub, $timeout) {
            var _this = this;
            this.edit = function (employee) {
                employee.Edit = true;
                _this.hub.invoke('lock', employee.Id);
            };
            this.done = function (employee) {
                employee.Edit = false;
                _this.hub.invoke('unlock', employee.Id);
            };
            // declaring the hub connection
            this.hub = new Hub('employee', {
                // client-side methods
                listeners: {
                    'lockEmployee': function (id) {
                        var employee = _this.find(id);
                        employee.Locked = true;
                        $rootScope.$apply();
                    },
                    'unlockEmployee': function (id) {
                        var employee = _this.find(id);
                        employee.Locked = false;
                        $rootScope.$apply();
                    }
                },
                // server-side methods
                methods: ['lock', 'unlock'],
                // query params sent on initial connection
                queryParams: {
                    'token': 'exampletoken'
                },
                // handle connection error
                errorHandler: function (message) {
                    console.error(message);
                },
                stateChanged: function (state) {
                    // your code here
                }
            });
        }
        EmployeesFactory.prototype.find = function (id) {
            for (var i = 0; i < this.all.length; i++) {
                if (this.all[i].Id === id)
                    return this.all[i];
            }
            return null;
        };
        EmployeesFactory.$inject = ['$rootScope', 'Hub', '$timeout'];
        return EmployeesFactory;
    })();
    ngSignalrTest.EmployeesFactory = EmployeesFactory;
})(ngSignalrTest || (ngSignalrTest = {}));
