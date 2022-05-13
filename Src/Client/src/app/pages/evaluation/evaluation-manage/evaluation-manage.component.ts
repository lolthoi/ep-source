import { QuarterEvaluationModel } from './../../../shared/models/quarter-evaluation.model';
import { MailService } from './../../../shared/services/mail.service';
import {
  DxButtonModule,
  DxFormModule,
  DxLoadPanelModule,
  DxNumberBoxModule,
  DxTextBoxModule,
  DxValidatorModule,
} from 'devextreme-angular';
import { Component, OnInit, NgModule } from '@angular/core';
import { CommonService } from 'src/app/shared/services/common.service';

@Component({
  selector: 'app-evaluation-manage',
  templateUrl: './evaluation-manage.component.html',
  styleUrls: ['./evaluation-manage.component.scss'],
})
export class EvaluationManageComponent implements OnInit {
  loadingVisible = false;
  quarterEvaluations: QuarterEvaluationModel[];

  now: Date = new Date();
  currentQuarter = Math.ceil((this.now.getMonth() + 1) / 3);
  currentYear = this.now.getFullYear();
  year: number;
  quarter: number;
  orderQuarter: string;

  constructor(
    private mailService: MailService,
    private common: CommonService
  ) {}
  ngOnInit(): void {
    this.year = this.currentYear;
    this.quarter = this.currentQuarter;
  }
  yearChange = (data) => {
    this.year = data.value;
  };
  quarterChange = (data) => {
    this.quarter = data.value;
  };
  convertOrderQuarter(q: number): string {
    if (q === 1) {
      return (this.orderQuarter = 'first');
    }
    if (q === 2) {
      return (this.orderQuarter = 'second');
    }
    if (q === 3) {
      return (this.orderQuarter = 'third');
    }
    return (this.orderQuarter = 'fourth');
  }
  onSendClick = (e) => {
    if (!this.year || !this.quarter) {
      this.common.UI.alertBox(
        'Input year and quarter before sending evaluation ',
        'Attention !'
      );
    } else {
      this.common.UI.confirmBox(
        'Do you want to send evaluation mail for the ' +
          this.convertOrderQuarter(this.quarter) +
          ' quarter of ' +
          this.year +
          '?',
        'Notice'
      ).then((result) => {
        if (result) {
          this.loadingVisible = true;
          this.mailService.sendMailAsync(this.year, this.quarter).subscribe(
            (res) => {
              this.loadingVisible = false;
              this.common.UI.multipleNotify(
                'Send evaluation mail success',
                'Success',
                4000
              );
            },
            (err) => {
              this.loadingVisible = false;
              if (
                err.error === 'Evaluation of this quarter has been already done'
              ) {
                this.common.UI.multipleNotify(
                  'Evaluation of this quarter has been already done',
                  'error',
                  4000
                );
              }
              if (err.error === 'No suitable project for evaluation') {
                this.common.UI.multipleNotify(
                  'No suitable project for evaluation',
                  'error',
                  4000
                );
              }
              if (err.error === 'No suitable employee for evaluation') {
                this.common.UI.multipleNotify(
                  'No suitable employee for evaluation',
                  'error',
                  4000
                );
              }
              if (err.error === 'Invalid input year or quarter') {
                this.common.UI.alertBox(
                  'Please input years from ' +
                    (this.currentYear - 5) +
                    ' to ' +
                    (this.currentYear + 5),
                  'Attention !'
                );
              }
              if (
                err.error ===
                'Sending evaluation mail will not proceed because you have not created enough evaluation criteria for all positions'
              ) {
                this.common.UI.multipleNotify(
                  'Sending evaluation mail will not proceed because you have not created enough evaluation criteria for all positions',
                  'error',
                  4000
                );
              }
            }
          );
        }
      });
    }
  };
  // onDeleteClick(e) {
  //   this.common.UI.confirmBox(
  //     'Do you want delete all existed quarter evaluation?',
  //     'Notice'
  //   ).then((result) => {
  //     this.loadingVisible = true;
  //     this.mailService.deleteGeneratedEvaluation().subscribe(
  //       (res) => {
  //         this.loadingVisible = false;
  //         this.common.UI.multipleNotify(
  //           'Delete all evaluation and employee self-evaluation success',
  //           'Success',
  //           4000
  //         );
  //       },
  //       (err) => {
  //         if (err.error === 'No any record in Quarter Evaluation') {
  //           this.loadingVisible = false;
  //           this.common.UI.multipleNotify(
  //             'Delete fail, no record in quarter evaluation',
  //             'error',
  //             4000
  //           );
  //         }
  //       }
  //     );
  //   });
  // }
}
@NgModule({
  imports: [
    DxButtonModule,
    DxLoadPanelModule,
    DxNumberBoxModule,
    DxTextBoxModule,
    DxFormModule,
    DxValidatorModule,
  ],
  declarations: [EvaluationManageComponent],
  bootstrap: [EvaluationManageComponent],
})
export class EvaluationManageModule {}
