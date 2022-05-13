import { UserService } from './../../services/user.service';
import { GoogleLoginProvider, SocialAuthService, SocialUser } from 'angularx-social-login';
import { CommonModule } from '@angular/common';
import { Component, NgModule } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { DxTextBoxModule, DxValidatorModule } from 'devextreme-angular';
import { DxFormModule } from 'devextreme-angular/ui/form';
import { DxLoadIndicatorModule } from 'devextreme-angular/ui/load-indicator';
import notify from 'devextreme/ui/notify';
import { AuthService } from '../../services';

@Component({
  selector: 'app-login-form',
  templateUrl: './login-form.component.html',
  styleUrls: ['./login-form.component.scss']
})
export class LoginFormComponent {
  loading = false;
  loadingWithGoogle = false;
  formData: any = {};
  passwordShowHideButtonComponent: any = null;

  Options = {
    passwordTextBoxOptions: {
      passwordMode: null,
      passwordButton: null,
      icon: null,
      showHideButtonVisible: null
    }
  }

  constructor(private authService: AuthService, private userService: UserService, private router: Router) {
    this.authService.logOut();
    //Init value
    this.Options.passwordTextBoxOptions.passwordMode = 'password';
    this.Options.passwordTextBoxOptions.icon = 'hidepassword';

    this.Options.passwordTextBoxOptions.passwordButton = {
      icon: this.Options.passwordTextBoxOptions.icon,
      type: "normal",
      visible: this.Options.passwordTextBoxOptions.showHideButtonVisible,
      onClick: (e) => {
        this.Options.passwordTextBoxOptions.passwordMode = this.Options.passwordTextBoxOptions.passwordMode === "text" ? "password" : "text";
        this.Options.passwordTextBoxOptions.icon = this.Options.passwordTextBoxOptions.passwordMode === "text" ? "showpassword" : "hidepassword";
        e.component.option("icon", this.Options.passwordTextBoxOptions.icon);
      },
      onInitialized: (e) => {
        this.passwordShowHideButtonComponent = e.component;
      }
    };

    //Init user sign-in google

    // this.googleUser = null;
    // this.authGoogleService.authState.subscribe((user: SocialUser) => {
    //   this.googleUser = user;
    // });
  }

  async signInWithGoogle() {
    const result = await this.authService.signInWithGoogle();
    if (!result.isOk) {
      this.loadingWithGoogle = false;
      notify(result.message, 'error', 5000);
    }
  }

  async onSubmit(e) {
    e.preventDefault();
    let { email, password } = this.formData;

    //Note: auto add "@kloon.vn" to the username if its not contains
    if (!email.toLowerCase().trim().endsWith("@kloon.vn")) {
      email = `${email.toLowerCase().trim()}@kloon.vn`;
    }

    this.loading = true;

    const result = await this.authService.logIn(email, password);
    if (!result.isOk) {
      this.loading = false;
      notify(result.message, 'error', 5000);
    }
  }

  onInputPasswordTextBox = (e) => {
    var changedValue = e.component.option("text");
    if (changedValue && changedValue.length > 0)
      this.Options.passwordTextBoxOptions.showHideButtonVisible = true;
    else
      this.Options.passwordTextBoxOptions.showHideButtonVisible = false;
    this.passwordShowHideButtonComponent.option("visible", this.Options.passwordTextBoxOptions.showHideButtonVisible)
  }

  onCreateAccountClick = () => {
    this.router.navigate(['/create-account']);
  }

  setFocus = (e) => {
    setTimeout(() => {
      e.component.focus();
    }, 0);
  }
}
@NgModule({
  imports: [
    CommonModule,
    RouterModule,
    DxFormModule,
    DxLoadIndicatorModule,
    DxTextBoxModule,
    DxValidatorModule
  ],
  declarations: [LoginFormComponent],
  exports: [LoginFormComponent]
})
export class LoginFormModule { }


