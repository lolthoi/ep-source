import { CommonModule } from "@angular/common";
import { Component, NgModule, OnInit, ViewChild } from "@angular/core";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { DxButtonModule, DxFormComponent, DxFormModule, DxLoadIndicatorModule, DxPopupModule } from "devextreme-angular";
import { ResetPasswordModel } from "../../models/reset-password.model";
import { AuthService } from "../../services";
import { CommonService } from "../../services/common.service";
import { custom } from "devextreme/ui/dialog";
import { UserService } from "../../services/user.service";


@Component({
  selector: 'app-change-passsword-form',
  templateUrl: './change-password-form.component.html'
})
export class ChangePasswordFormComponent implements OnInit {

  popupVisible = false;
  changePassword: ResetPasswordModel = new ResetPasswordModel();
  logOutClick = 1;
  disableSubmitButton = false;
  @ViewChild(DxFormComponent, { static: false }) myForm: DxFormComponent;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private commonService: CommonService
  ) { }

  ngOnInit() {
  }

  setFocus(e) {

    this.myForm.instance.getEditor('password').focus();
  }

  //#region Options
  closeButtonOptions = {
    text: 'Cancel',
    icon: 'close',
    onClick: (e) => {
      this.myForm.instance._refresh();
      this.myForm.instance.repaint();
      this.popupVisible = false;
    },
  };
  saveButtonOptions = {
    text: 'Change Password',
    icon: 'edit',
    onClick: async (e) => {
      if (this.myForm.instance.validate().isValid) {
        await this.submitForm();
      }
    }
  }
  ////#endregion

  open() {
    this.changePassword = new ResetPasswordModel();
    this.popupVisible = true;
  }

  confirmPassword = (e: { value: string }) => {
    return e.value === this.changePassword.newPassword;
  }
  async submitForm() {
    this.disableSubmitButton = true;
    (await this.userService.changedPassword(this.changePassword))
      .subscribe(
        (response: ResetPasswordModel) => {
          this.disableSubmitButton = false;
          var changedPasswordDialog = custom({
            title: "Success",
            messageHtml: "<p>Change password successfully. Click on the Log Out button to Log Out.</p>",
            buttons: [{
              text: "Log Out",
              onClick: (e) => {
                this.authService.logOut();
                this.logOutClick = 2;
              }
            }]
          });
          changedPasswordDialog.show();

          setTimeout(() => {
            if (this.logOutClick === 1) {
              changedPasswordDialog.hide();
              this.authService.logOut();
            }
          }, 15000);

        },
        (error) => {
          this.commonService.UI.toastMessage(error.error, 'error', 5000);
          this.disableSubmitButton = false;
        }
      )

  }

}
@NgModule({
  imports: [
    CommonModule,
    RouterModule,
    DxFormModule,
    DxButtonModule,
    DxPopupModule,
    DxLoadIndicatorModule,
    DxPopupModule
  ],
  declarations: [ChangePasswordFormComponent],
  exports: [ChangePasswordFormComponent]
})
export class ChangePasswordFormModule { }
