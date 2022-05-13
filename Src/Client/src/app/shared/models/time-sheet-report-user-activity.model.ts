export class TimeSheetReportUserActivityModel {
  userId: number;
  fullName: string;
  activityId: string;
  displayName: string;
  groupName: string;
  activityName: string;
  totalHour: number | null;
}

export class FilterParamUserActivity {
  uIds: number[];
  pIds: number[];
  activityIds: string[];
  startDate: Date;
  endDate: Date;

  constructor(init?: Partial<FilterParamUserActivity>) {
    Object.assign(this, init);
  }
}
