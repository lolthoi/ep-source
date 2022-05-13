import { AppRolesEnum } from './user-app.model';
export class UserModel {
    id: number;
    email: string;
    firstName: string;
    lastName: string;
    positionId: number;
    sex: EnumUserSex;
    doB: Date;
    phoneNo: any;
    roleId: AppRolesEnum;
    status: boolean;

    constructor(init?: Partial<UserModel>) {
        Object.assign(this, init);
    }
}

export enum EnumUserSex {
    MALE = 1,
    FEMALE = 2
}