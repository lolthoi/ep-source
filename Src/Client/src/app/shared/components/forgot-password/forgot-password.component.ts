import { CommonModule } from '@angular/common';
import { Component, NgModule } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { DxFormModule, DxButtonModule, DxLoadIndicatorModule } from 'devextreme-angular';
import notify from 'devextreme/ui/notify';
import { AuthService } from '../../services';
import { CommonService } from '../../services/common.service';
import { ForgotPasswordModel } from "./../../models/forgot-password.model";

const notificationText = 'We\'ve sent a link to reset your password. Check your inbox.';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss']
})
export class ForgotPasswordComponent {

  loading = false;
  forgotPassword: ForgotPasswordModel = new ForgotPasswordModel();
  errorMessage = "";
  isSuccess: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private commonService: CommonService
  ) {
    this.forgotPassword.url = window.location.origin;
  }

  async onSubmit(e) {
    this.errorMessage = "";
    e.preventDefault();
    (await this.authService.forgotPassword(this.forgotPassword)).subscribe(
      (response: ForgotPasswordModel) => {
        this.commonService.UI.toastMessage("Please check your email to reset password", "success", 3000);
        this.isSuccess = true;
      },
      (error) => {
        this.errorMessage = error.error;
      }
    )
  }

}
@NgModule({
  imports: [
    CommonModule,
    RouterModule,
    DxFormModule,
    DxLoadIndicatorModule,
    DxButtonModule
  ],
  declarations: [ForgotPasswordComponent],
  exports: [ForgotPasswordComponent]
})
export class ForgotPasswordFormModule { }
