export class TimeSheetReportProjectActivityModel {
  groupId: string;
  groupName: string;
  activityId: string;
  activityName: string;
  totalHour: number | null;
  projectId: number | null;
}

export class FilterParamProjectActivity {
  groupIds: string[];
  activityIds: string[];
  startDate: Date;
  endDate: Date;

  constructor(init?: Partial<FilterParamProjectActivity>) {
    Object.assign(this, init);
  }
}
