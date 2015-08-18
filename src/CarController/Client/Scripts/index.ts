///<reference path="typing\angularjs\angular.d.ts"/>
///<reference path="typing\angular-signalr-hub\angular-signalr-hub.d.ts"/>


module App {
    export interface MainScope extends ng.IScope {
        states: string;
        connectedCar: App.Structure.Car;
        cars: App.Structure.Car[];
        connect: Function;
        disconnect: Function;
        commands: App.Structure.Command[];
        verbs: string[];
        add: Function;
        remove: Function;
        allRemove: Function;
        checkLength: Function;
        sent: Function;
        abort: Function;
    }

    export class MainController {
        constructor(public $scope: MainScope, public proxy: HubConnection) {
            $scope.states = "disconnected";
            $scope.connectedCar = undefined;
            $scope.cars = [];
            $scope.commands = [];
            $scope.verbs = [];
            for (var item in App.Structure.Verb) {
                var result = parseInt(item, 10);
                if (result !== result) {
                    $scope.verbs.push(item);
                }
            }

            $scope.connect = angular.bind(this, this.connect);
            $scope.disconnect = angular.bind(this, this.disconnect);
            $scope.add = angular.bind(this, this.add);
            $scope.remove = angular.bind(this, this.remove);
            $scope.allRemove = angular.bind(this, this.allRemove);
            $scope.checkLength = angular.bind(this, this.checkLength);
            $scope.sent = angular.bind(this, this.sent);
            $scope.abort = angular.bind(this, this.abort);

            proxy.ChangeCars = cars=> {
                $scope.cars = cars;
                if ($scope.states == "connected") {
                    for (var car in this.$scope.cars) {
                        if ($scope.cars[car].ConnectedID == $scope.connectedCar.ConnectedID) {
                            $scope.connectedCar = $scope.cars[car];
                            break;
                        }
                    }
                }
                $scope.$apply();
            }

            proxy.SentDisconnectFromCar = () => {
                this.$scope.states = "disconnected";
                this.$scope.$apply();
            }
        }

        connect(id: string): void {
            if (this.$scope.states == "connected") {
                this.disconnect();
            }

            this.proxy.connectToCar(id).done(result=> {
                if (result) {
                    this.$scope.states = "connected";
                    for (var car in this.$scope.cars) {
                        if (this.$scope.cars[car].ConnectedID == id) {
                            this.$scope.connectedCar = this.$scope.cars[car];
                            break;
                        }
                    }
                    this.$scope.commands = [];
                } else {
                    this.$scope.states = "disconnected";
                    this.$scope.connectedCar = undefined;
                }
            });
            this.$scope.states = "connecting";
        }

        disconnect(): void {
            this.proxy.disconnectFromCar();
            this.$scope.states = "disconnected";
            this.$scope.$apply();
        }

        add(): void {
            this.$scope.commands.push({ Time: 20, Verb: App.Structure.Verb.Stop });
        }

        remove(index: number): void {
            this.$scope.commands.splice(index, 1);
        }

        allRemove(): void {
            this.$scope.commands = [];
        }

        checkLength(value: number, index: number): boolean {
            let length: number = 0;
            for (var i = 0; i < this.$scope.commands.length; i++) {
                if (i == index) {
                    length += value;
                } else {
                if (this.$scope.commands[i].Time != undefined) {
                    length += this.$scope.commands[i].Time;
                }
                }
            }
            return length <= 60;
        }

        sent(): void {
            this.proxy.sentCommandsToCar(this.$scope.commands);
        }

        abort(): void {
            this.proxy.abortExcuting();
        }
    }

    export class HubConnection {
        private connection: ngSignalr.Hub;

        public ChangeCars: (managers: App.Structure.Car[]) => void;
        public SentDisconnectFromCar: () => void;

        constructor(private Hub: ngSignalr.HubFactory) {
            this.connection = new Hub("CentralHost", {
                rootPath: "signalr", errorHandler: error=> alert(error),
                listeners: {
                    "SentDisconnectFromCar": ()=> {
                        this.SentDisconnectFromCar();
                    },
                    "ChangeCars": (cars: App.Structure.Car[]) => {
                        this.ChangeCars(cars);
                    }
                },
                stateChanged: state => {
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
            this.connection.invoke("ConectAsClient");
            this.connection.invoke("GetCarList").done((cars: App.Structure.Car[]) => this.ChangeCars(cars));
        }

        connectToCar(id: string): JQueryDeferred<boolean> {
            return this.connection.invoke("ConectToCar", id);
        }
        disconnectFromCar(): void {
            this.connection.invoke("DisconnectFromCar");
        }
        sentCommandsToCar(commands: App.Structure.Command[]): JQueryDeferred<boolean> {
            return this.connection.invoke("SentCommandsToCar", commands);
        }
        abortExcuting(): void {
            this.connection.invoke("AbortExcuting");
        }
    }
}
angular.module("app.service", ["SignalR"])
    .service("proxy", App.HubConnection);


angular.module("app", ["app.service", "ngAnimate","ui.validate"])
    .controller("controller", App.MainController);