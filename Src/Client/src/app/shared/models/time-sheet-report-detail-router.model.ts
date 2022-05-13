export class TimeSheetReportDetailRouterModel {
  page: number = 1;
  pageSize: number = 10;
  userIds: number[];
  projectIds: number[];
  tSAcitivityGroupIds: string[];
  taskIds: string[];
  startDate: Date;
  endDate: Date;

  constructor(init?: Partial<TimeSheetReportDetailRouterModel>) {
    Object.assign(this, init);
  }
}
