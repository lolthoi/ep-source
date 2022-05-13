export class TimeSheetRecordModel {
  id: string;
  tsActivityId: string;
  userId: number;
  name: string;
  backlogId: string;
  taskId: string;
  startTime: Date;
  endTime: Date;

  constructor(init?: Partial<TimeSheetRecordModel>) {
    Object.assign(this, init);
  }
}

export class ActivityRecordViewModel {
  id: string;
  displayName: string;
  groupId: string;
  groupName: string;
  activityName: string;
  deletedBy: number;
}

export class ActivityGroupRecordViewModel {
  groupName: string;
  activityRecordViewModels: ActivityRecordViewModel[];
}
