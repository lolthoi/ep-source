import * as internal from "stream";

export class WorkSpaceSettingModel {
    isLockTimeSheet: string;
    lockAfter: string;
    lockValueByDate: string;
}

export class WorkSpaceSettingForm {
    lockTime: boolean;
    lockingTime: any;
    timesheetLockCycle: number;
    constructor(init?: Partial<WorkSpaceSettingForm>) {
        Object.assign(this, init);
    }
}

export class WorkSpaceSettingRecordModel{
    isLockTimeSheet: boolean;
    lockAfter: string
    lockValueByDate: number;
}