import { TimeSpan } from './timepan';

export class TimeSheetReportDetailModel {
  userId: number;
  firstName: string;
  lastName: string;

  backlogId: string;
  taskId: string;
  tsAcitivityGroupId: string;
  tsActivityId: string;
  tsAcitivityGroupName: string;
  tSActivityName: string;
  timeSheetRecordId: string;
  timeSheetRecordName: string;

  startDate: Date;
  startHour: string;
  endDate: Date;
  endHour: string;
  dateRecord: string;

  nameTask: string;
  duration: TimeSpan;
  totalTime: TimeSpan;
  fullName: string;

  constructor(init?: Partial<TimeSheetReportDetailModel>) {
    Object.assign(this, init);
  }
}
