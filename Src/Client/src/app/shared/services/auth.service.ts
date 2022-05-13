import {
  AppRolesEnum,
  ProjectRolesEnum,
  UserApp,
} from './../models/user-app.model';
import { UserService } from './user.service';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { JwtHelperService } from '@auth0/angular-jwt';
import { ForgotPasswordModel } from '../models/forgot-password.model';
import { ResetPasswordModel } from '../models/reset-password.model';
import {
  GoogleLoginProvider,
  SocialAuthService,
  SocialUser,
} from 'angularx-social-login';

const defaultPath = '/home';
const defaultUser = {
  email: 'sandra@example.com',
  avatarUrl:
    'https://js.devexpress.com/Demos/WidgetsGallery/JSDemos/images/employees/06.png',
};

@Injectable()
export class AuthService {
  // private _user = null;
  googleUser: SocialUser | null;

  private userSubject: BehaviorSubject<UserApp>;
  public user: Observable<UserApp>;

  get loggedIn(): boolean {
    return !!this.userSubject.value;
  }

  private _lastAuthenticatedPath: string = defaultPath;
  set lastAuthenticatedPath(value: string) {
    this._lastAuthenticatedPath = value;
  }

  constructor(
    private router: Router,
    private httpClient: HttpClient,
    private userService: UserService,
    private authGoogleService: SocialAuthService
  ) {
    this.userSubject = new BehaviorSubject<UserApp>(
      JSON.parse(localStorage.getItem('user'))
    );
    this.user = this.userSubject.asObservable();
  }

  async logIn(email: string, password: string) {
    try {
      return this.userService
        .login(email, password)
        .toPromise()
        .then(
          (res) => {
            let user = new UserApp(res);
            this.userSubject.next(user);
            localStorage.setItem('user', JSON.stringify(res));
            if (this._lastAuthenticatedPath == 'error')
              this._lastAuthenticatedPath = 'home';
            this.router.navigate([this._lastAuthenticatedPath]);
            return {
              isOk: true,
              data: user,
              message: '',
            };
          },
          (err) => {
            let msgError =
              typeof err.error == 'string' ? err.error : err.message;
            return {
              isOk: false,
              data: this.userSubject.value,
              message: msgError,
            };
          }
        );
    } catch {
      return {
        isOk: false,
        message: 'Authentication failed',
      };
    }
  }

  async signInWithGoogle() {
    try {
      var googleIdToken = (
        await this.authGoogleService.signIn(GoogleLoginProvider.PROVIDER_ID)
      ).idToken;
      //CHECK EMAIL BELONG TO KLOON?

      //SEND GOOGLETOKENID TO EXTERNAL API

      return this.userService
        .externalLogin(googleIdToken)
        .toPromise()
        .then(
          (next) => {
            let user = new UserApp(next);
            this.userSubject.next(user);
            localStorage.setItem('user', JSON.stringify(user));
            if (this._lastAuthenticatedPath == 'error')
              this._lastAuthenticatedPath = 'home';
            this.router.navigate([this._lastAuthenticatedPath]);
            return {
              isOk: true,
              data: user,
              message: '',
            };
          },
          (err) => {
            let msgError =
              typeof err.error == 'string' ? err.error : err.message;
            return {
              isOk: false,
              data: this.userSubject.value,
              message: msgError,
            };
          }
        );
    } catch {
      return {
        isOk: false,
        message: 'Authentication failed',
      };
    }
  }

  signOutGoogle(): void {
    this.authGoogleService.signOut();
  }

  get getUserValue() {
    return this.userSubject.value;
  }

  get getUser() {
    return this.userSubject.value;
  }

  public onChangeUserValue(object: any) {
    let userValue = this.userSubject.value;
    for (var property in object) {
      if (Object.prototype.hasOwnProperty.call(userValue, property)) {
        userValue[property] = object[property];
      }
    }
    this.userSubject.next(userValue);
  }

  get isRoleAdministrator() {
    return this.userSubject.value.appRole == AppRolesEnum.ADMINISTRATOR
      ? true
      : false;
  }

  get isProjectLeader() {
    let result: boolean = false;
    let projectRoles = this.userSubject.value.projectRoles;
    let userRole = this.userSubject.value.appRole;

    if (userRole == AppRolesEnum.ADMINISTRATOR) return (result = true);
    if (projectRoles.length == 0) return result;

    projectRoles.forEach((t) => {
      if (t.projectRoleId == ProjectRolesEnum.PM) result = true;
    });

    return result;
  }

  async createAccount(email, password) {
    try {
      // Send request
      this.router.navigate(['/create-account']);
      return {
        isOk: true,
      };
    } catch {
      return {
        isOk: false,
        message: 'Failed to create account',
      };
    }
  }

  async changePassword(email: string, recoveryCode: string) {
    try {
      // Send request
      return {
        isOk: true,
      };
    } catch {
      return {
        isOk: false,
        message: 'Failed to change password',
      };
    }
  }

  async forgotPassword(entity: ForgotPasswordModel) {
    return this.httpClient.post<ForgotPasswordModel>(
      '/Account/ForgotPassword',
      entity
    );
  }
  async resetPassword(entity: ResetPasswordModel) {
    return this.httpClient.post<ResetPasswordModel>(
      '/Account/ResetPassword',
      entity
    );
  }
  public emailIsValid(email: string) {
    return this.httpClient.get<boolean>(
      `/Account/ForgotPassword?email=${email}`
    );
  }

  public codeForResetPassword(code: string) {
    return this.httpClient.get<boolean>(
      `/Account/CodeForResetPassword?code=${code}`
    );
  }

  async logOut() {
    localStorage.removeItem('user');
    this.userSubject.next(null);
    this.router.navigate(['/login-form']);
    if (this.googleUser) this.signOutGoogle();
  }
}

@Injectable()
export class AuthGuardService implements CanActivate {
  constructor(
    private router: Router,
    private authService: AuthService,
    private jwtHelperService: JwtHelperService
  ) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const isLoggedIn = this.authService.loggedIn;
    const isAuthForm = [
      'login-form',
      'reset-password',
      'forgot-password',
      'create-account',
      'change-password/:recoveryCode',
    ].includes(route.routeConfig.path);
    const isLeaderPages = ['evaluation-leader'].includes(
      route.routeConfig.path
    );

    if (isLoggedIn && isAuthForm) {
      this.authService.lastAuthenticatedPath = defaultPath;
      this.router.navigate([defaultPath]);
      return false;
    }

    if (!isLoggedIn && !isAuthForm) {
      //If access to pages inside app and not login yet
      this.authService.lastAuthenticatedPath = route.routeConfig.path;
      this.router.navigate(['/login-form']);
    }

    const isAuthorizeRouting = this.isAuthorized(route.data.allowedRoles);

    //If loggedin and access leader page only => redirect to error page
    if (isLoggedIn && isLeaderPages) {
      let isRoleProjectLeader = this.authService.isProjectLeader;
      let isAdmin = this.authService.isRoleAdministrator;
      if (!isRoleProjectLeader && !isAdmin) {
        this.router.navigate(['/error']);
      }
    }

    if (isLoggedIn && isAuthorizeRouting) {
      this.authService.lastAuthenticatedPath = route.routeConfig.path;
    }

    if (isLoggedIn && !isAuthorizeRouting) {
      this.router.navigate([defaultPath]);
    }

    return isLoggedIn || isAuthForm;
  }

  isAuthorized(allowedRoles: Int32Array[]): boolean {
    // check if the list of allowed roles is empty, if empty, authorize the user to access the page
    if (allowedRoles == null || allowedRoles.length === 0) {
      return true;
    }

    // get token from local storage or state management
    const token = this.authService.getUserValue.token;

    // decode token to read the payload details
    const decodeToken = this.jwtHelperService.decodeToken(token);

    // check if it was decoded successfully, if not the token is not valid, deny access
    if (!decodeToken) {
      return false;
    }

    // check if the user roles is in the list of allowed roles, return true if allowed and false if not allowed
    return allowedRoles.includes(
      decodeToken[
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
      ]
    );
  }
}
