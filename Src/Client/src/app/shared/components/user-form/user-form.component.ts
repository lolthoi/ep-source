import { PositionModel } from './../../models/position.model';
import { PositionService } from './../../services/position.service';
import { AppRolesEnum } from './../../models/user-app.model';
import { EnumUserSex } from './../../models/user.model';
import {
  Component,
  EventEmitter,
  Input,
  NgModule,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import {
  DxButtonModule,
  DxDataGridModule,
  DxFormComponent,
  DxFormModule,
  DxPopupModule,
  DxValidatorModule,
  DxTextBoxModule,
  DxScrollViewModule,
} from 'devextreme-angular';
import { UserModel } from '../../models/user.model';
import { UserService } from '../../services/user.service';
import { AuthService } from '../../services';
import { CommonService } from '../../services/common.service';
import { JwtHelperService } from '@auth0/angular-jwt';

@Component({
  selector: 'app-user-form',
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.scss'],
})
export class UserFormComponent implements OnInit {
  @Input() model: UserFormModel;
  @Input() position: PositionModel[];
  @Output() onRefreshGrid = new EventEmitter<void>();
  @ViewChild(DxFormComponent, { static: false }) myform: DxFormComponent;

  emailPattern: any = /^\s*[A-Za-z0-9-.\\+]+(\\.[_A-Za-z0-9-]+)*@(K|k)(L|l)(O|o)(O|o)(N.|n.)(V|v)(N|n)\s*$/;
  phonePattern: any = /([0-9]{10})/;
  isAdminRole = false;
  formState = FormState;
  popupVisible = false;
  popupConfirmDeleteVisible = false;
  popupTitle = '';
  currUser: UserModel;
  titleChange: any;
  disableSubmitButton = false;
  sexDataSource = [
    { caption: 'Male', value: EnumUserSex.MALE },
    { caption: 'Female', value: EnumUserSex.FEMALE },
  ];

  roleDataSource = [
    { caption: 'ADMINISTRATOR', value: AppRolesEnum.ADMINISTRATOR },
    { caption: 'USER', value: AppRolesEnum.USER },
  ];

  constructor(private userService: UserService, private authService: AuthService, private common: CommonService, private jwtHelperService: JwtHelperService) {
    this.isAdminRole = this.authService.isRoleAdministrator;
  }

  open() {
    switch (this.model.state) {
      case FormState.CREATE:
        this.popupTitle = 'CREATE USER';
        break;
      case FormState.DETAIL:
        this.popupTitle = 'DETAIL USER';
        break;
      case FormState.EDIT:
        this.popupTitle = 'EDIT USER';
        break;
    }
    this.currUser = this.model.data;
    if (this.myform) {
      this.myform.instance.resetValues();
    }
    this.popupVisible = true;
  }

  setFocus(e) {
    // if (this.popupVisible == true) {
    //   this.myform.instance._refresh();
    //   this.myform.instance.repaint();
    // }

    this.myform.instance.getEditor("firstName").focus();
  }

  //#region Options
  closeButtonOptions = {
    text: 'Cancel',
    icon: 'close',
    onClick: (e) => {
      this.popupVisible = false;
    },
  };

  createButtonOptions = {
    icon: 'save',
    text: 'Save',
    onClick: (e) => {
      var instance = this.myform.instance.validate();
      if (!instance.isValid) {
        return;
      }
      this.disableSubmitButton = true;

      this.userService.add(this.currUser).subscribe(
        (next) => {
          this.common.UI.multipleNotify('Successfully Added', 'Success', 2000);
          this.popupVisible = false;
          this.onRefreshGrid.emit();
          this.disableSubmitButton = false;
        },
        (err) => {
          if (err.error === 'INVALID_MODEL_DUPLICATED_EMAIL') {
            this.common.UI.multipleNotify('Email is existed !', 'error', 2000);
          }
          this.disableSubmitButton = false;
        }
      );
     
    },
  };

  deleteButtonOptions = {
    icon: 'trash',
    text: 'Delete',
    onClick: (e) => {
      this.popupConfirmDeleteVisible = true;
    },
  };

  enterEditFormButtonOptions = {
    icon: 'edit',
    text: 'Edit',
    onClick: (e) => {
      this.model.state = FormState.EDIT;
      this.popupTitle = 'EDIT USER';
      this.myform.instance.getEditor("firstName").focus();
    },
  };

  editButtonOptions = {
    icon: 'save',
    text: 'Save',
    onClick: (e) => {
      var instance = this.myform.instance.validate();
      if (!instance.isValid) {
        return;
      }
      this.disableSubmitButton = true;

      this.userService.edit(this.currUser).subscribe(
        (next) => {
          //#region Temp solution for
          let decodedToken = this.jwtHelperService.decodeToken(
            this.authService.getUserValue.token
          );
          const currentLoggedInUserId =
            decodedToken[
            'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid'
            ];
          const currentLoggedInUserRoleId =
            decodedToken[
            'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
            ];

          if (currentLoggedInUserId == this.currUser.id) {
            this.authService.onChangeUserValue(this.currUser);
          }
          if (
            this.currUser.roleId != currentLoggedInUserRoleId &&
            this.currUser.id == currentLoggedInUserId
          ) {
            this.authService.logOut();
          }
          //#endregion

          //TODO: Call refresh grid
          this.popupVisible = false;
          this.onRefreshGrid.emit();
          this.disableSubmitButton = false;
          this.common.UI.multipleNotify('Update User Success', 'Success', 2000);
        },
        (e: any) => {
          let errerMess = 'Update failed: ' + e.error;
          switch (e.error) {
            case 'INVALID_MODEL_DUPLICATED_EMAIL':
              errerMess = 'Email is existed !';
              break;
            case 'DEACTIVED_LAST_ADMIN':
              errerMess = 'Can not Deactive the last Admintrator!!!';
              break;
            default:
              break;
          }
          this.common.UI.multipleNotify(errerMess, 'error', 2000);
          this.disableSubmitButton = false;
        }
      );
    },
  };

  closeDeletePopupButtonOptions = {
    text: 'Cancel',
    icon: 'close',
    onClick: (e) => {
      this.popupConfirmDeleteVisible = false;
    },
  };

  confirmDeleteButtonOptions = {
    icon: 'save',
    text: 'Ok',
    onClick: (e) => {
      this.userService.delete(this.currUser.id).subscribe(
        (next) => {
          this.popupConfirmDeleteVisible = false;
          this.popupVisible = false;
          this.common.UI.multipleNotify('Delete User Success', 'Success', 2000);
          this.onRefreshGrid.emit();
        },
        (e: any) => {
          if (e.error === 'Cannot delete yourself') {
            this.common.UI.multipleNotify(
              'Action denied...Cannot delete yourself !',
              'error',
              2000
            );
          }
        }
      );
    },
  };
  ////#endregion

  ngOnInit(): void { }
}

@NgModule({
  imports: [
    BrowserModule,
    DxDataGridModule,
    DxButtonModule,
    DxPopupModule,
    DxFormModule,
    DxTextBoxModule,
    DxValidatorModule,
    DxScrollViewModule,

  ],
  declarations: [UserFormComponent],
  exports: [UserFormComponent],
})
export class UserFormModule { }

export class UserFormModel {
  state: FormState;
  data: UserModel;

  constructor(init?: Partial<UserFormModel>) {
    Object.assign(this, init);
  }
}

export enum FormState {
  DETAIL,
  CREATE,
  EDIT,
}
