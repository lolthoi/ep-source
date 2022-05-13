import {
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
} from '@angular/core';
import { TimeSheetRecordService } from 'src/app/shared/services/time-sheet-record.service';

@Component({
  selector: 'time-sheet-entry-group-header',
  templateUrl: './time-sheet-entry-group-header.component.html',
  styleUrls: ['./time-sheet-entry-group-header.component.scss'],
})
export class TimeSheetEntryGroupHeaderComponent implements OnInit, OnChanges {
  @Input() date;
  @Input() entries;
  now: Date = new Date();
  shownDate: string;
  totalHour;

  constructor(private tsRecordService: TimeSheetRecordService) {}
  ngOnChanges(changes: SimpleChanges): void {
    this.calculateTotalTime(this.entries);
  }

  ngOnInit(): void {
    this.calculateTotalTime(this.entries);
    this.InitConvertDate();
  }
  InitConvertDate() {
    var a = this.date.split('/');
    if (a[1] < 10) {
      if (a[0] < 10) {
        this.shownDate = '0' + a[1] + '/' + '0' + a[0] + '/' + a[2];
      } else {
        this.shownDate = '0' + a[1] + '/' + a[0] + '/' + a[2];
      }
    } else {
      if (a[0] < 10) {
        this.shownDate = a[1] + '/' + '0' + a[0] + '/' + a[2];
      } else {
        this.shownDate = a[1] + '/' + a[0] + '/' + a[2];
      }
    }
  }
  private calculateTotalTime(entries) {
    let totalEntriesDiffMillisecond = entries.reduce((acc, cur) => {
      let diffMillisecond = this.tsRecordService.calculateDiffMs(
        cur.startTime,
        cur.endTime
      );
      return acc + diffMillisecond;
    }, 0);
    this.totalHour = this.tsRecordService.msToTime(totalEntriesDiffMillisecond);
  }
}
