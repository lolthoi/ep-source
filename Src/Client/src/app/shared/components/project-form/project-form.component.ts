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
  DxSelectBoxModule,
  DxTabPanelModule,
  DxScrollViewModule,
} from 'devextreme-angular';

import { confirm } from 'devextreme/ui/dialog';
import { ProjectUserModel } from '../../models/project-user.model';
import { ProjectModel } from '../../models/project.model';
import { UserApp } from '../../models/user-app.model';
import { UserModel } from '../../models/user.model';
import { AuthService } from '../../services';
import { CommonService } from '../../services/common.service';
import { ProjectUserService } from '../../services/project-user.service';
import DataSource from 'devextreme/data/data_source';
import { stringify } from '@angular/compiler/src/util';

@Component({
  selector: 'app-project-form',
  templateUrl: './project-form.component.html',
  styleUrls: ['./project-form.component.scss'],
})
export class ProjectFormComponent implements OnInit {
  @Input() model: ProjectFormModel;
  projectUserFormModel: ProjectUserFormModel = new ProjectUserFormModel();
  @Input() selectedIndex = 0;
  @Output() onSubmitForm: EventEmitter<ProjectModel> =
    new EventEmitter<ProjectModel>();
  @Output() onConfirmDelete: EventEmitter<boolean> =
    new EventEmitter<boolean>();
  @ViewChild(DxFormComponent, { static: false }) myForm: DxFormComponent;

  selectBoxListUsers: UserNotInProject[] = [];
  dataSource: ProjectUserModel[];
  gridColumns = ['no', 'email', 'firstName', 'lastName', 'projectRole'];
  loading = false;
  listUserNotInProject: UserModel[];
  userSelect: UserModel;
  userSelectId = 0;
  dataSourceSelectBox: DataSource;

  formState = ProjectFormState;
  popupVisible = false;
  popupTitle = '';
  currentProject = new ProjectModel();

  isAdminRole = false;

  popupVisibleProjectUser = false;
  popupTitleProjectUser = '';
  userCurrent: UserApp = null;

  popDetailComp: any;
  selectBoxUserComp: any;
  buttonAddUserComp: any;
  initTextboxNameComp: any;
  projectUserState = ProjectUserState;
  searchTextUser = '';
  popupConfirmDeleteProjectVisible = false;
  popupConfirmDeleteProjectUserVisible = false;
  popupSaveProjectVisible = false;

  public disableSubmitButton = false;
  constructor(
    private projectMemberService: ProjectUserService,
    private authService: AuthService,
    private commonService: CommonService
  ) {
    this.isAdminRole = this.authService.isRoleAdministrator;
    this.userCurrent = this.authService.getUser;
  }
  ngOnInit(): void {
    this.dataSourceSelectBox = new DataSource({
      load: async (options: any) => {
        const text = options === null ? '' : options.searchValue;
        if (!text) return [];
        const data = await this.projectMemberService
          .GetTopFiveUserNotInProject(this.currentProject.id, text)
          .toPromise();
        return data;
      },
    });
  }

  listStatus: ProjectStatus[] = [
    {
      id: 1,
      status: 'Open',
    },
    {
      id: 2,
      status: 'Pending',
    },
    {
      id: 3,
      status: 'Closed',
    },
  ];

  listProjectUserRole: ProjectRoleStatus[] = [
    {
      id: 1,
      status: 'Member',
    },
    {
      id: 2,
      status: 'QA',
    },
    {
      id: 3,
      status: 'Project Manager',
    },
  ];

  open(isNext: boolean) {
    switch (this.model.state) {
      case ProjectFormState.CREATE:
        this.popupTitle = 'CREATE PROJECT';
        break;

      case ProjectFormState.EDIT:
        this.popupTitle = 'UPDATE PROJECT';
        break;
      case ProjectFormState.DETAIL:
        this.popupTitle = 'DETAIL PROJECT';
        break;
    }
    this.popupVisible = true;
    this.currentProject = Object.assign({}, this.model.data);
    this.selectedIndex = isNext ? 1 : 0;
    this.getProjectMember();

    this.OnInitDataUser('');

    this.userSelectId = 0;
    this.searchTextUser = '';
    this.selectBoxListUsers = [];
  }
  close() {
    this.popupVisible = false;
  }

  // #region Options

  closeButtonOption = {
    text: 'Cancel',
    hint: 'Cancel',
    icon: 'remove',
    onClick: (e) => {
      this.popupVisible = false;
    },
  };

  createButtonOptions = {
    icon: 'save',
    text: 'Add',
    hint: 'Add',
    onClick: (e) => {
      const valid = this.myForm.instance.validate();
      if (!valid.isValid) {
        return;
      }
      this.disableSubmitButton = true;
      this.onSubmitForm.emit(this.currentProject);
    },
  };

  editButtonOptions = {
    icon: 'edit',
    hint: 'Update',
    text: 'Update',
    onClick: (e) => {
      this.disableSubmitButton = true;
      this.onSave(false);
    },
  };

  editButtonOnDetailOptions = {
    icon: 'edit',
    hint: 'Edit',
    text: 'Edit',
    onClick: (e) => {
      this.popupTitle = 'Update Project';
      this.model.state = ProjectFormState.EDIT;
      this.selectedIndex = 0;
    },
  };

  deleteButtonOnDetailOptions = {
    icon: 'trash',
    hint: 'Delete',
    text: 'Delete',
    onClick: (e) => {
      this.popupConfirmDeleteProjectVisible = true;
    },
  };

  confirmDeletePopupProjectButtonOptions = {
    icon: 'save',
    text: 'Ok',
    onClick: (e) => {
      this.onConfirmDelete.emit(true);
      this.popupConfirmDeleteProjectVisible = false;
    },
  };

  closeDeletePopupProjectButtonOptions = {
    text: 'Cancel',
    icon: 'close',
    onClick: (e) => {
      this.popupConfirmDeleteProjectVisible = false;
    },
  };

  setFocus(e) {
    this.popDetailComp.repaint();
    if (this.model.state === ProjectFormState.CREATE) {
      this.myForm.instance.getEditor('name').focus();
    }
  }

  onValidationCallBackStartDate = (e: any) => {
    if (e === undefined) {
      return true;
    }
    if (
      this.currentProject.endDate === null ||
      this.currentProject.endDate === undefined
    ) {
      return true;
    }
    return this.compareDateStartDate(
      new Date(e.value),
      new Date(this.currentProject.endDate)
    );
  };

  onValidationCallBackEndDate = (e: any) => {
    if (e === undefined && e === null) {
      return true;
    }
    if (e.value === undefined || e.value === null) {
      return true;
    }
    if (
      this.currentProject.startDate === null ||
      this.currentProject.startDate === undefined
    ) {
      return true;
    }
    return this.compareDateEndDate(
      new Date(this.currentProject.startDate),
      new Date(e.value)
    );
  };

  compareDateStartDate(startDate: Date, endDate: Date) {
    if (startDate < endDate) {
      this.myForm.instance.getEditor('endDate').option('isValid', true);
      return true;
    } else {
      if (
        this.myForm.instance.getEditor('endDate').option('isValid') === false
      ) {
        return true;
      } else {
        return false;
      }
    }
  }

  compareDateEndDate(startDate: Date, endDate: Date) {
    if (startDate < endDate) {
      this.myForm.instance.getEditor('startDate').option('isValid', true);
      return true;
    } else {
      if (
        this.myForm.instance.getEditor('startDate').option('isValid') === false
      ) {
        return true;
      } else {
        return false;
      }
    }
  }

  onValueChangedStartDate = (e: any) => {
    this.currentProject.startDate = e.value;
  };

  onValueChangedEndDate = (e: any) => {
    this.currentProject.endDate = e.value;
  };

  // #endregion

  //#region tab 2

  OnInitDataUser = (keySearch: string = '') => {
    this.selectBoxListUsers = [];
    if (this.selectBoxUserComp != null) {
      this.selectBoxUserComp.option('items', []);
    }
    if (keySearch === '') {
      return;
    }

    this.projectMemberService
      .GetTopFiveUserNotInProject(this.currentProject.id, keySearch)
      .subscribe(
        (response: UserModel[]) => {
          response.forEach((i, index) => {
            if (this.selectBoxListUsers.length < response.length) {
              this.selectBoxListUsers.push({
                id: i.id,
                fullName: i.firstName + ' ' + i.lastName + '(' + i.email + ')',
              });
            }
          });
          this.selectBoxUserComp.option('items', this.selectBoxListUsers);
        },
        (error) => {
          console.log(error);
        }
      );
  };

  getProjectMember() {
    if (this.currentProject.id === 0) {
      return;
    }
    this.projectMemberService
      .GetProjectMember(this.currentProject.id)
      .subscribe(
        (responeseData: ProjectUserModel[]) => {
          this.dataSource = [];
          if (responeseData.length > 0) {
            this.dataSource = responeseData;
            this.dataSource.forEach((element, index) => {
              element.no = index + 1;
            });
          }
          this.loading = false;
        },
        (error) => {
          this.loading = false;
          this.commonService.UI.multipleNotify(error.error, 'error', 2000);
        }
      );
  }

  onToolbarPreparing(e) {
    e.toolbarOptions.items.unshift({
      location: 'before',
      locateInMenu: 'auto',
      template: 'myToolbarTemplate',
    });
  }

  onInitSelectBoxUser(e) {
    this.selectBoxUserComp = e.component;
  }

  onValueChangedDopdownUser = (e: any) => {
    this.userSelectId = e.value;
  };
  onOptionChangedDopdownUser = (e: any) => {
    if (e.name === 'isActive' && e.previousValue && !e.value) {
      this.dataSourceSelectBox.load();
    }
  };
  onKeyUpDopdownUser = (e: any) => {
    // var search = e.component.option('text');
    // this.selectBoxUserComp.option('text', search);
    // this.OnInitDataUser(search);
  };

  onInitButtonAddUser(e) {
    this.buttonAddUserComp = e.component;
  }

  onAddProjectMember(e): void {
    var projectId = this.model.data.id;
    var userId = this.userSelectId;
    if (!userId) {
      this.commonService.UI.multipleNotify(
        'Please choose the user to add to the Project.',
        'error',
        2000
      );
      return;
    }
    this.projectMemberService.add(projectId, userId).subscribe(
      (responeseData: ProjectUserModel) => {
        this.commonService.UI.multipleNotify(
          'Add a member to project success.',
          'success',
          2000
        );
        this.getProjectMember();
        this.selectBoxUserComp.option('value', null);
        this.selectBoxUserComp.option('items', []);
        this.OnInitDataUser('');
      },
      (error) => {
        this.loading = false;
        this.commonService.UI.multipleNotify(error.error, 'error', 2000);
        this.popDetailComp.repaint();
      }
    );
  }

  onOpenDetailProjectUserButton(e, data): void {
    this.projectMemberService
      .GetProjectMemberById(this.currentProject.id, data.data.id)
      .subscribe(
        (responeseData: ProjectUserModel) => {
          this.projectUserFormModel.state = ProjectUserState.DETAIL;
          this.projectUserFormModel.data = new ProjectUserModel(responeseData);
          this.openPopupUserModel();
        },
        (error) => {
          this.loading = false;
          this.commonService.UI.multipleNotify(error.error, 'error', 2000);
        }
      );
  }

  openPopupUserModel() {
    switch (this.projectUserFormModel.state) {
      case ProjectUserState.EDIT:
        this.popupTitleProjectUser = 'UPDATE PROJECT MEMBER';
        break;
      case ProjectUserState.DETAIL:
        this.popupTitleProjectUser = 'DETAIL PROJECT MEMBER';
        break;
    }
    this.popupVisibleProjectUser = true;
  }

  btnSubmitProjectUserOptions = {
    icon: 'save',
    text: 'Update',
    onClick: (e) => {
      this.projectMemberService
        .edit(
          this.projectUserFormModel.data.projectId,
          this.projectUserFormModel.data.id,
          this.projectUserFormModel.data.projectRoleId
        )
        .subscribe(
          (responeseData: ProjectUserModel) => {
            this.commonService.UI.multipleNotify(
              'Update a member to project success.',
              'success',
              2000
            );
            this.popupVisibleProjectUser = false;
            this.getProjectMember();
          },
          (error) => {
            this.loading = false;
            this.commonService.UI.multipleNotify(error.error, 'error', 2000);
          }
        );
    },
  };

  isProjectleaderProject(): boolean {
    if (this.userCurrent == null) {
      return false;
    }

    return this.isAdminRole == true;
  }

  btnEditProjectUserOptions = {
    icon: 'edit',
    text: 'Edit',
    onClick: (e) => {
      this.popupTitleProjectUser = 'Update Project Member';
      this.projectUserFormModel.state = ProjectUserState.EDIT;
    },
  };

  deleteButtonOnDetailProjectUserOptions = {
    icon: 'trash',
    text: 'Delete',
    onClick: (e) => {
      this.popupConfirmDeleteProjectUserVisible = true;
    },
  };

  closeButtonProjectUserOption = {
    text: 'Cancel',
    icon: 'remove',
    onClick: (e) => {
      this.popupVisibleProjectUser = false;
    },
  };

  okSaveProjectButtonOptions = {
    icon: 'save',
    text: 'Yes',
    onClick: (e) => {
      this.onSave(true);

      this.selectedIndex = 1;
      if (this.selectBoxUserComp) {
        this.selectBoxUserComp.focus();
      }
      this.popupSaveProjectVisible = false;
    },
  };

  cancelSaveProjectButtonOptions = {
    text: 'No',
    icon: 'close',
    onClick: (e) => {
      this.popupSaveProjectVisible = false;
    },
  };

  onChangeTab = (e: any) => {
    const selectIndex = e.component.option('selectedIndex');
    if (this.onCheckChange() && selectIndex === 1) {
      this.popupSaveProjectVisible = true;
    }
    setTimeout(() => {
      this.selectedIndex = selectIndex;
      if (this.selectBoxUserComp) {
        this.selectBoxUserComp.focus();
      }
    }, 200);
  };
  onInitPopUpDetail = (e: any) => {
    this.popDetailComp = e.component;
  };
  onSave = (isNextTab) => {
    const valid = this.myForm.instance.validate();
    if (!valid.isValid) {
      if (isNextTab) {
        this.commonService.UI.toastMessage(
          'Save error ' + valid.brokenRules[0].message,
          'error',
          2000
        );
      }
      this.disableSubmitButton = false;
      return;
    }
    this.onSubmitForm.emit(this.currentProject);
  };
  onCheckChange = () => {
    // Observable.of(obj1.value)
    //   .pipe(
    //     map((objValue: string) => {
    //       return { value: objValue };
    //     })
    //   )
    //   .subscribe((value: { value: string }) => {
    //     this.compararison = value === obj1;
    //   });
    const readModel = JSON.stringify(this.model.data);
    const model = JSON.stringify(this.currentProject);
    return readModel !== model;
  };

  confirmDeletePopupProjectUserButtonOptions = {
    icon: 'save',
    text: 'Ok',
    onClick: (e) => {
      this.projectMemberService
        .delete(
          this.projectUserFormModel.data.projectId,
          this.projectUserFormModel.data.id
        )
        .subscribe(
          () => {
            this.commonService.UI.multipleNotify(
              'Delete Project Member Success',
              'success',
              2000
            );
            this.popupVisibleProjectUser = false;
            this.getProjectMember();

            this.OnInitDataUser('');
            this.popupConfirmDeleteProjectUserVisible = false;
          },
          (error) => {
            this.loading = false;
            this.commonService.UI.multipleNotify(error.error, 'error', 5000);
            this.popupConfirmDeleteProjectUserVisible = false;
          }
        );
    },
  };

  closeDeletePopupProjectUserButtonOptions = {
    text: 'Cancel',
    icon: 'close',
    onClick: (e) => {
      this.popupConfirmDeleteProjectUserVisible = false;
    },
  };
}

//#endregion

@NgModule({
  imports: [
    BrowserModule,
    DxDataGridModule,
    DxButtonModule,
    DxPopupModule,
    DxFormModule,
    DxSelectBoxModule,
    DxTabPanelModule,
    DxScrollViewModule,
  ],
  declarations: [ProjectFormComponent],
  exports: [ProjectFormComponent],
})
export class ProjectFormModule {}

export class ProjectFormModel {
  state: ProjectFormState;
  data: ProjectModel;

  constructor(init?: Partial<ProjectFormModel>) {
    Object.assign(this, init);
  }
}

export enum ProjectFormState {
  CREATE,
  EDIT,
  DETAIL,
}

export class ProjectStatus {
  id: number;
  status: string;
}

export class ProjectUserFormModel {
  state: ProjectUserState;
  data: ProjectUserModel;

  constructor(init?: Partial<ProjectUserFormModel>) {
    Object.assign(this, init);
  }
}

export enum ProjectUserState {
  EDIT,
  DETAIL,
}
export class ProjectRoleStatus {
  id: number;
  status: string;
}

export class UserNotInProject {
  id: number;
  fullName: string;
}
