/// <reference path="angular-route-1.2.d.ts" />
$routeProvider
    .when('/projects/:projectId/dashboard', {
    controller: '',
    templateUrl: '',
    caseInsensitiveMatch: true,
    reloadOnSearch: false
})
    .otherwise({ redirectTo: '/' });
