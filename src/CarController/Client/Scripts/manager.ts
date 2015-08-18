///<reference path="typing\angularjs\angular.d.ts"/>
///<reference path="typing\angular-signalr-hub\angular-signalr-hub.d.ts"/>
///<reference path="typing/jquery/jquery.d.ts"/>


module App.Manager {
    export interface MainScope extends ng.IScope {
        messages: string[];
        managers: App.Structure.Manager[];
        clients: App.Structure.Client[];
        cars: App.Structure.Car[];
        disconnectFromClient: Function;
        disconnectCar: Function;
        shutdownCar: Function;
        rebootCar: Function;
        reset: Function;
    }

    export class MainController {
        constructor(public $scope: MainScope, public proxy: HubConnection) {
            $scope.messages = [];
            $scope.managers = [];
            $scope.clients = [];
            $scope.cars = [];

            $scope.reset = angular.bind(this, this.reset);
            $scope.disconnectFromClient = angular.bind(this, this.disconnectFromClient);
            $scope.disconnectCar = angular.bind(this, this.disconnectCar);
            $scope.shutdownCar = angular.bind(this, this.shutdownCar);
            $scope.rebootCar = angular.bind(this, this.rebootCar);

            proxy.Sent = message=> {
                $scope.messages.push(message);
                $scope.$apply();
            };

            proxy.ChangeManagers = managers=> {
                $scope.managers = managers;
                $scope.$apply();
            }

            proxy.ChangeClients = clients=> {
                $scope.clients = clients;
                $scope.$apply();
            }

            proxy.ChangeCars = cars=> {
                $scope.cars = cars;
                $scope.$apply();
            }

        }

        reset(): void {
            this.$scope.messages = [];
            this.$scope.$apply();
        }

        disconnectFromClient(id: string): void {
            this.proxy.disconnectFromClient(id);
        }

        disconnectCar(id: string): void {
            this.proxy.disconnectCar(id);
        }

        shutdownCar(id: string): void {
            this.proxy.shutdownCar(id);
        }

        rebootCar(id: string): void {
            this.proxy.rebootCar(id);
        }
    }

    export class HubConnection {
        private connection: ngSignalr.Hub;
        public Sent: (message: string) => void;
        public ChangeManagers: (managers: App.Structure.Manager[]) => void;
        public ChangeClients: (managers: App.Structure.Client[]) => void;
        public ChangeCars: (managers: App.Structure.Car[]) => void;


        constructor(private Hub: ngSignalr.HubFactory) {
            this.connection = new Hub("CentralHost", {
                rootPath: "signalr", errorHandler: error=> alert(error),
                listeners: {
                    "Sent": message=> {
                        this.Sent(message);
                    },
                    "ChangeManagers": (managers: App.Structure.Manager[]) => {
                        this.ChangeManagers(managers);
                    },
                    "ChangeClients": (clients: App.Structure.Client[]) => {
                        this.ChangeClients(clients);
                    },
                    "ChangeCars": (cars: App.Structure.Car[]) => {
                        this.ChangeCars(cars);
                    }
                },
                stateChanged: state =>
                {
                    switch (state.newState) {
                        case $.signalR.connectionState.connecting:
                            //your code here
                            break;
                        case $.signalR.connectionState.connected:
                            this.connect();
                            break;
                        case $.signalR.connectionState.reconnecting:
                            //your code here
                            break;
                        case $.signalR.connectionState.disconnected:
                            //your code here
                            break;
                    }
                }
            });
        }
        connect(): void {
            this.connection.invoke("ConnectAsManager");
            this.connection.invoke("GetManagerList").done((managers: App.Structure.Manager[]) => this.ChangeManagers(managers));
            this.connection.invoke("GetClientList").done((clients: App.Structure.Client[]) => this.ChangeClients(clients));
            this.connection.invoke("GetCarList").done((cars: App.Structure.Car[]) => this.ChangeCars(cars));
        }

        public disconnectFromClient(id: string): void {
            this.connection.invoke("DisconnectFromCar", id);
        }

        public disconnectCar(id: string): void {
            this.connection.invoke("DisconnectCar", id);
        }

        public shutdownCar(id: string): void {
            this.connection.invoke("ShutdownCar", id);
        }

        public rebootCar(id: string): void {
            this.connection.invoke("RebootCar", id);
        }
    }
}
angular.module("app.manager.service", ["SignalR"])
    .service("proxy", App.Manager.HubConnection);


angular.module("app.manager", ["app.manager.service"])
    .controller("controller", App.Manager.MainController);