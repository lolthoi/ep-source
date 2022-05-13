export class ActivityGroupUserModel {
  id: string;
  activityGroupId: string;
  userId: number;
  role: TsRole;
  fullName: string;
  email: string;
  constructor(init?: Partial<ActivityGroupUserModel>) {
    Object.assign(this, init);
  }
}

export enum TsRole {
  MANAGER = 1,
  QA = 2,
}
