import { DxFormModule } from 'devextreme-angular/ui/form';
import { Component, NgModule, OnInit } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { TimeTrackerRecorderComponent } from 'src/app/shared/components/time-sheet/time-tracker-recorder/time-tracker-recorder.component';
import {
  DxDateBoxModule,
  DxTextBoxModule,
  DxButtonModule,
  DxCalendarModule,
  DxValidatorModule,
  DxSelectBoxModule,
  DxPopupModule,
  DxValidationSummaryModule,
} from 'devextreme-angular';
import { NgxPaginationModule } from 'ngx-pagination';
import { TimeSheetRecordService } from 'src/app/shared/services/time-sheet-record.service';
import { TimeSheetEntryGroupComponent } from 'src/app/shared/components/time-sheet/time-sheet-entry-group/time-sheet-entry-group.component';
import { TimeSheetEntryGroupHeaderComponent } from 'src/app/shared/components/time-sheet/time-sheet-entry-group/time-sheet-entry-group-header/time-sheet-entry-group-header.component';
import { TimeSheetEntryComponent } from 'src/app/shared/components/time-sheet/time-sheet-entry-group/time-sheet-entry/time-sheet-entry.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

@Component({
  selector: 'app-time-tracker',
  templateUrl: './time-tracker.component.html',
  styleUrls: ['./time-tracker.component.scss'],
})
export class TimeTrackerComponent implements OnInit {
  tsGroupEntries: any;
  page = 1;
  count = 0;
  pageSize = 50;
  pageSizes = [50, 100, 150];
  keys: any; //array keys of object key/value pair

  constructor(private tsRecordService: TimeSheetRecordService) {}

  ngOnInit(): void {
    this.getRecords();
  }

  //Get params for request Get
  getParamRequest(searchText, page, pageSize) {
    let params = {};

    if (searchText) {
      params[`searchText`] = searchText;
    }

    if (page) {
      params[`page`] = page;
    }

    if (pageSize) {
      params[`pageSize`] = pageSize;
    }

    return params;
  }

  getRecords() {
    const params = this.getParamRequest('', this.page, this.pageSize);
    this.tsRecordService.getAll(params).subscribe(
      (next: any) => {
        this.count = next.totalCount;
        this.tsGroupEntries =
          this.tsRecordService.convertListToKeyValuePairObject(next.items);
        this.keys = Object.keys(this.tsGroupEntries);
      },
      (error) => {}
    );
  }

  //#region Handle paginator event
  handlePageChange(event) {
    this.page = event;
    this.getRecords();
  }

  handlePageSizeChange(event) {
    this.pageSize = event.target.value;
    this.page = 1;
    this.getRecords();
  }
  //#endregion
}

@NgModule({
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    DxPopupModule,
    DxFormModule,
    DxTextBoxModule,
    DxDateBoxModule,
    DxButtonModule,
    DxCalendarModule,
    DxValidatorModule,
    DxSelectBoxModule,
    NgxPaginationModule,
    DxValidatorModule,
    DxValidationSummaryModule,
  ],
  declarations: [
    TimeTrackerComponent,
    TimeTrackerRecorderComponent,
    TimeSheetEntryGroupComponent,
    TimeSheetEntryGroupHeaderComponent,
    TimeSheetEntryComponent,
  ],
  providers: [],
})
export class TimeTrackerModule {}
