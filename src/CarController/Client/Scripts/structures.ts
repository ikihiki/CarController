module App.Structure {
    export interface Car {
        ConnectedID: string;
        IsConnected: boolean;
        ConnectedClient: string;
        Name: string;
    }

    export interface Client {
        ConnectedID: string;
        IsConnected: boolean;
        ConnectedCar: string;
    }

    export interface Manager {
        ConnectedID: string;
    }

    export interface Command {
        Verb: Verb;
        Time: number;
    }

    export enum Verb {
        Stop,
        Fowerd,
        FowerdRight,
        FowerdLeft,
        Back,
        BackRight,
        BackLeft
    }
}