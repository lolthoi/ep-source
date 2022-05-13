import { CommonModule } from '@angular/common';
import { Component, NgModule } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DxFormModule } from 'devextreme-angular/ui/form';
import { DxLoadIndicatorModule } from 'devextreme-angular/ui/load-indicator';
import { AuthService } from '../../services';
import { ResetPasswordModel } from '../../models/reset-password.model';
import { CommonService } from '../../services/common.service';

const notificationText = 'We\'ve sent a link to reset your password. Check your inbox.';

@Component({
  selector: 'app-reset-password-form',
  templateUrl: './reset-password-form.component.html',
  styleUrls: ['./reset-password-form.component.scss']
})
export class ResetPasswordFormComponent {
  loading = false;
  resetPassword: ResetPasswordModel = new ResetPasswordModel();
  code: string = "";

  statusCode: number = StatusResePassword.INVALID;
  modelState = StatusResePassword;

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private commonService: CommonService
  ) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.code = params['code'];
      this.resetPassword.code = this.code;
      this.authService.codeForResetPassword(this.code)
        .subscribe(
          (response: boolean) => {
            this.statusCode = response == true ? StatusResePassword.INVALID : StatusResePassword.VALID;
          }
        )
    });


  }

  async onSubmit(e) {
    this.loading = true;
    e.preventDefault();
    (await this.authService.resetPassword(this.resetPassword))
      .subscribe(
        (response: ResetPasswordModel) => {
          this.commonService.UI.toastMessage("Your Password has been reset", "success", 5000);
          this.statusCode = StatusResePassword.SUCCESS;
          this.loading = false;
        },
        (error) => {
          this.commonService.UI.toastMessage(error.error, 'error', 2000);
          this.loading = false;
        }
      )

  }

  confirmPassword = (e: { value: string }) => {
    return e.value === this.resetPassword.newPassword;
  }
}
@NgModule({
  imports: [
    CommonModule,
    RouterModule,
    DxFormModule,
    DxLoadIndicatorModule
  ],
  declarations: [ResetPasswordFormComponent],
  exports: [ResetPasswordFormComponent]
})
export class ResetPasswordFormModule { }

export enum StatusResePassword {
  INVALID = 1,
  VALID = 2,
  SUCCESS = 3
}