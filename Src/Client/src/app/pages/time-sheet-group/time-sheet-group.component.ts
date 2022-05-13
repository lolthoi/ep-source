import { UserModel } from 'src/app/shared/models/user.model';
import { ActivityGroupUserService } from './../../shared/services/activity-group-user.service';
import { ActivityService } from './../../shared/services/activity.service';
import {
  ActivityGroupUserModel,
  TsRole,
} from './../../shared/models/activity-group-user.model';
import { ActivityModel } from './../../shared/models/activity.model';
import {
  DxTabPanelModule,
  DxPopupModule,
  DxButtonModule,
  DxDataGridModule,
  DxSelectBoxModule,
  DxFormModule,
  DxFormComponent,
  DxScrollViewModule,
  DxValidatorModule,
  DxValidationSummaryModule,
} from 'devextreme-angular';
import { ActivityGroupModel } from './../../shared/models/activity-group.model';
import { Component, NgModule, OnInit, ViewChild } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ActivityGroupService } from 'src/app/shared/services/activity-group.service';
import { CommonService } from 'src/app/shared/services/common.service';
import { UserService } from 'src/app/shared/services/user.service';

@Component({
  selector: 'app-time-sheet-group',
  templateUrl: './time-sheet-group.component.html',
  styleUrls: ['./time-sheet-group.component.scss'],
})
export class TimeSheetGroupComponent implements OnInit {
  @ViewChild(DxFormComponent, { static: false }) myform: DxFormComponent;
  model: ActivityGroupUserFormModel = new ActivityGroupUserFormModel();
  activityGroups: ActivityGroupModel[];
  activityGroup = new ActivityGroupModel();
  groupDetailPopup = false;
  groupTitle: any;
  activities: any;
  activity = new ActivityModel();
  users: ActivityGroupUserModel[] = [];
  user = new ActivityGroupUserModel();
  popupTitle = '';

  buttonAddUserComp: any;
  detailUserDataSource = new ActivityGroupUserModel();
  dataSourceSelectBox: any;
  resultSelectBox: UserModel[] = [];
  popupGroupUser = false;

  formState = FormState;
  userSelectId = 0;
  userRole = 0;

  tsRoleDataSource = [
    { caption: 'Manager', value: TsRole.MANAGER },
    { caption: 'QA', value: TsRole.QA },
  ];

  constructor(
    private activityGroupService: ActivityGroupService,
    private activityService: ActivityService,
    private activityGroupUserService: ActivityGroupUserService,
    private userService: UserService,
    private common: CommonService
  ) {}

  InitActivityGroup() {
    this.activityGroupService.GetAll().subscribe(
      (res) => {
        this.activityGroups = res;
      },
      (err) => {}
    );
  }
  ngOnInit(): void {
    this.InitActivityGroup();
    this.InitUserDataSource();
  }

  onCreate(e: any) {
    this.activityGroup = {
      id: e.key,
      projectId: null,
      name: e.data.name,
      description: e.data.description,
      activities: null,
      activityGroupUserModels: null,
    };
    this.activityGroupService.Create(this.activityGroup).subscribe(
      (res) => {
        this.InitActivityGroup();
        this.common.UI.multipleNotify('Create Group Success', 'success', 2000);
      },
      (err) => {
        this.InitActivityGroup();
        if (err.error === 'Name is required') {
          this.common.UI.multipleNotify(
            'Group Name is required',
            'error',
            4000
          );
        }
        if (err.error === 'Project Name existed') {
          this.common.UI.multipleNotify(
            'This activity group already exists',
            'error',
            4000
          );
        }
        if (err.error === 'Activity Group Name existed') {
          this.common.UI.multipleNotify(
            'This activity group already exists',
            'error',
            4000
          );
        }
        if (err.error === 'Name character is too short or too long') {
          this.common.UI.multipleNotify(
            'Group Name is too short or too long',
            'error',
            4000
          );
        }
      }
    );
  }
  onUpdate(e: any) {
    this.activityGroup = {
      id: e.data.id,
      projectId: e.data.projectId,
      name: e.data.name,
      description: e.data.description,
      activities: null,
      activityGroupUserModels: null,
    };
    this.activityGroupService.Update(this.activityGroup).subscribe(
      (res) => {
        this.InitActivityGroup();
        this.common.UI.multipleNotify('Update Group Success', 'success', 2000);
      },
      (err) => {
        this.InitActivityGroup();
        if (err.error === 'Inediteable Group') {
          this.common.UI.multipleNotify('Group is inediteable', 'error', 4000);
        }
        if (err.error === 'Name is required') {
          this.common.UI.multipleNotify(
            'Group Name is required',
            'error',
            4000
          );
        }
        if (err.error === 'Project Name existed') {
          this.common.UI.multipleNotify(
            'This activity group already exists',
            'error',
            4000
          );
        }
        if (err.error === 'Activity Group Name existed') {
          this.common.UI.multipleNotify(
            'This activity group already exists',
            'error',
            4000
          );
        }
        if (err.error === 'Name character is too short or too long') {
          this.common.UI.multipleNotify(
            'Group Name is too short or too long',
            'error',
            4000
          );
        }
      }
    );
  }
  onDelete(e: any) {
    this.activityGroupService.Delete(e.data.id).subscribe(
      (res) => {
        this.InitActivityGroup();
        this.common.UI.multipleNotify('Delete Group Success', 'success', 4000);
      },
      (err) => {
        this.InitActivityGroup();
        if (err.error === 'Activity Group not found') {
          this.common.UI.multipleNotify(
            'Activity Group not found',
            'error',
            4000
          );
        }
        if (err.error === 'Undeleteable Group') {
          this.common.UI.multipleNotify('Group is undeleteable', 'error', 4000);
        }
      }
    );
  }
  opOpenGroupDetail(e: any, data: any) {
    this.activityGroupService.GetById(data.data.id).subscribe(
      (res) => {
        this.activityGroup = res;
        this.groupDetailPopup = true;
        this.groupTitle = this.activityGroup.name;
        this.activities = this.activityGroup.activities;
        this.users = this.activityGroup.activityGroupUserModels;
      },
      (err) => {
        if (err.error === 'Activity Group not found') {
          this.common.UI.multipleNotify(
            'Activity Group not found',
            'error',
            4000
          );
        }
      }
    );
  }
  closeButtonOptions = {
    text: 'Close',
    icon: 'close',
    onClick: (e) => {
      this.groupDetailPopup = false;
    },
  };
  onCreateActivity(e: any) {
    this.activity = {
      id: e.key,
      activityGroupId: this.activityGroup.id,
      name: e.data.name,
      description: e.data.description,
    };
    this.activityService.Create(this.activity).subscribe(
      (res) => {
        this.activityGroupService
          .GetById(this.activityGroup.id)
          .subscribe((res) => {
            this.activities = res.activities;
          });
        this.common.UI.multipleNotify(
          'Create Activity Success',
          'success',
          2000
        );
      },
      (err) => {
        this.activityGroupService
          .GetById(this.activityGroup.id)
          .subscribe((res) => {
            this.activities = res.activities;
          });
        if (err.error === 'Activity Group not found') {
          this.common.UI.multipleNotify(
            'Activity Group not found',
            'error',
            4000
          );
        }
        if (err.error === 'Name is required') {
          this.common.UI.multipleNotify('Name is required', 'error', 4000);
        }
        if (err.error === 'Activity Name existed') {
          this.common.UI.multipleNotify(
            'This activity already exists',
            'error',
            4000
          );
        }
        if (err.error === 'Name character is too short or too long') {
          this.common.UI.multipleNotify(
            'Activity Name is too short or too long',
            'error',
            4000
          );
        }
      }
    );
  }
  onUpdateActivity(e: any) {
    this.activity = {
      id: e.key,
      activityGroupId: this.activityGroup.id,
      name: e.data.name,
      description: e.data.description,
    };
    this.activityService.Update(this.activity).subscribe(
      (res) => {
        this.activityGroupService
          .GetById(this.activityGroup.id)
          .subscribe((res) => {
            this.activities = res.activities;
          });
        this.common.UI.multipleNotify(
          'Update Activity Success',
          'success',
          2000
        );
      },
      (err) => {
        this.activityGroupService
          .GetById(this.activityGroup.id)
          .subscribe((res) => {
            this.activities = res.activities;
          });
        if (err.error === 'Activity Group not found') {
          this.common.UI.multipleNotify(
            'Activity Group not found',
            'error',
            4000
          );
        }
        if (err.error === 'Activity not found') {
          this.common.UI.multipleNotify('Activity not found', 'error', 4000);
        }
        if (err.error === 'Name is required') {
          this.common.UI.multipleNotify('Name is required', 'error', 4000);
        }
        if (err.error === 'Activity Name existed') {
          this.common.UI.multipleNotify(
            'This activity already exists',
            'error',
            4000
          );
        }
        if (err.error === 'Name character is too short or too long') {
          this.common.UI.multipleNotify(
            'Activity Name is too short or too long',
            'error',
            4000
          );
        }
      }
    );
  }
  onDeleteActivity(e: any) {
    this.activityService.Delete(e.data.id).subscribe(
      (res) => {
        this.activityGroupService
          .GetById(this.activityGroup.id)
          .subscribe((res) => {
            this.activities = res.activities;
          });
        this.common.UI.multipleNotify(
          'Delete Activity Success',
          'success',
          4000
        );
      },
      (err) => {
        this.activityGroupService
          .GetById(this.activityGroup.id)
          .subscribe((res) => {
            this.activities = res.activities;
          });
        if (err.error === 'Activity not found') {
          this.common.UI.multipleNotify(
            'Activity Group not found',
            'error',
            4000
          );
        }
      }
    );
  }
  onToolbarPreparing(e) {
    e.toolbarOptions.items.unshift({
      location: 'after',
      locateInMenu: 'auto',
      template: 'myToolbarTemplate',
    });
  }

  onInitButtonAddUser(e: any) {
    this.buttonAddUserComp = e.component;
  }
  InitUserDataSource() {
    this.userService.getUsers('').subscribe((res) => {
      var noAdmin = res.filter((x) => x.roleId !== 1);
      this.dataSourceSelectBox = noAdmin;
    });
  }
  onAddGroupUser(e: any) {
    this.popupGroupUser = true;
    this.popupTitle = 'Add Access Group User';
    this.model.state = this.formState.CREATE;
    if (this.myform) {
      this.myform.instance.resetValues();
    }
    this.detailUserDataSource = new ActivityGroupUserModel();
  }
  SaveAddedButtonOptions = {
    icon: 'save',
    text: 'Save',
    onClick: (e) => {
      var instance = this.myform.instance.validate();
      if (!instance.isValid) {
        return;
      }
      this.user.activityGroupId = this.activityGroup.id;
      this.user.userId = this.detailUserDataSource.userId;
      this.user.role = this.detailUserDataSource.role;
      this.activityGroupUserService.Create(this.user).subscribe(
        (res) => {
          this.activityGroupService
            .GetById(this.activityGroup.id)
            .subscribe((res) => {
              this.users = res.activityGroupUserModels;
            });
          this.common.UI.multipleNotify(
            'Add access User success',
            'success',
            2000
          );
          this.popupGroupUser = false;
        },
        (err) => {
          this.activityGroupService
            .GetById(this.activityGroup.id)
            .subscribe((res) => {
              this.users = res.activityGroupUserModels;
            });
          if (err.error === 'Activity not found') {
            this.common.UI.multipleNotify(
              'Activity Group not found',
              'error',
              4000
            );
          }
          if (err.error === 'UserId not found') {
            this.common.UI.multipleNotify('UserId not found', 'error', 4000);
          }
          if (err.error === 'Invalid TSRole') {
            this.common.UI.multipleNotify('Role not existed', 'error', 4000);
          }
          if (err.error === 'Duplicated Group User') {
            this.common.UI.multipleNotify(
              'This access User already exists',
              'error',
              4000
            );
          }
        }
      );
    },
  };

  onOpenDetailGroupUser(e: any, data) {
    this.activityGroupUserService.GetById(data.data.id).subscribe((res) => {
      this.detailUserDataSource = res;
      this.popupGroupUser = true;
      this.popupTitle = 'Detail Access Group User';
      this.model.state = this.formState.DETAIL;
    });
  }
  DeleteButtonOptions = {
    icon: 'trash',
    text: 'Delete',
    onClick: (e) => {
      this.common.UI.confirmBox(
        'Do you want to delete ' + this.detailUserDataSource.fullName + ' ?',
        'Notice'
      ).then((result) => {
        if (result) {
          this.activityGroupUserService
            .Delete(this.detailUserDataSource.id)
            .subscribe(
              (res) => {
                this.activityGroupService
                  .GetById(this.activityGroup.id)
                  .subscribe((res) => {
                    this.users = res.activityGroupUserModels;
                  });
                this.common.UI.multipleNotify(
                  'Delete Access User Success',
                  'success',
                  2000
                );
                this.popupGroupUser = false;
              },
              (err) => {
                this.activityGroupService
                  .GetById(this.activityGroup.id)
                  .subscribe((res) => {
                    this.users = res.activityGroupUserModels;
                  });
                if (err.error === 'Activity Group User not found') {
                  this.common.UI.multipleNotify(
                    'Activity Group User not found',
                    'error',
                    4000
                  );
                }
              }
            );
        }
      });
    },
  };
  enterEditFormButtonOptions = {
    icon: 'edit',
    text: 'Edit',
    onClick: (e) => {
      this.model.state = this.formState.EDIT;
    },
  };
  SaveEditedButtonOptions = {
    icon: 'save',
    text: 'Update',
    onClick: (e) => {
      this.activityGroupUserService.Update(this.detailUserDataSource).subscribe(
        (res) => {
          this.activityGroupService
            .GetById(this.activityGroup.id)
            .subscribe((res) => {
              this.users = res.activityGroupUserModels;
            });
          this.common.UI.multipleNotify(
            'Update Access User Success',
            'success',
            2000
          );
          this.popupGroupUser = false;
        },
        (err) => {
          this.activityGroupService
            .GetById(this.activityGroup.id)
            .subscribe((res) => {
              this.users = res.activityGroupUserModels;
            });
          if (err.error === 'Activity Group not found') {
            this.common.UI.multipleNotify(
              'Activity Group not found',
              'error',
              4000
            );
          }
          if (err.error === 'Activity Group User not found') {
            this.common.UI.multipleNotify(
              'Activity Group User not found',
              'error',
              4000
            );
          }
          if (err.error === 'UserId not found') {
            this.common.UI.multipleNotify('UserId not found', 'error', 4000);
          }
          if (err.error === 'Invalid TSRole') {
            this.common.UI.multipleNotify('Role not existed', 'error', 4000);
          }
          if (err.error === 'Duplicated Group User') {
            this.common.UI.multipleNotify(
              'Duplicated Group User',
              'error',
              4000
            );
          }
        }
      );
    },
  };
  closeGroupUserDetail = {
    text: 'Close',
    icon: 'close',
    onClick: (e) => {
      this.popupGroupUser = false;
    },
  };
  closeGroupUserAddEdit = {
    text: 'Cancel',
    icon: 'close',
    onClick: (e) => {
      this.popupGroupUser = false;
    },
  };
}

@NgModule({
  imports: [
    BrowserModule,
    DxDataGridModule,
    DxButtonModule,
    DxPopupModule,
    DxTabPanelModule,
    DxSelectBoxModule,
    DxFormModule,
    DxScrollViewModule,
    DxValidatorModule,
    DxValidationSummaryModule,
  ],
  declarations: [TimeSheetGroupComponent],
  exports: [TimeSheetGroupComponent],
})
export class TimeSheetGroupModule {}

export class ActivityGroupUserFormModel {
  state: FormState;
  data: ActivityGroupUserModel;
  constructor(init?: Partial<ActivityGroupUserFormModel>) {
    Object.assign(this, init);
  }
}
export enum FormState {
  DETAIL,
  CREATE,
  EDIT,
}
