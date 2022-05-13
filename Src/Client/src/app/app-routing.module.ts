import { TimeSheetGroupComponent } from './pages/time-sheet-group/time-sheet-group.component';
import { EvaluationLeaderComponent } from './pages/evaluation-leader/evaluation-leader.component';
import { ErrorComponent } from './pages/error/error.component';
import { ProjectComponent } from './pages/project/project.component';
import { UserComponent } from './pages/user/user.component';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {
  LoginFormComponent,
  ResetPasswordFormComponent,
  CreateAccountFormComponent,
  ChangePasswordFormComponent,
  ForgotPasswordComponent,
} from './shared/components';
import { AuthGuardService } from './shared/services';
import { HomeComponent } from './pages/home/home.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { DxDataGridModule, DxFormModule } from 'devextreme-angular';
import { CriteriasComponent } from './pages/criterias/criterias.component';
import { AppRoutingRole } from './shared/common/enum-app-routing-role';
import { EvaluationPersonalComponent } from './pages/evaluation/evaluation-personal/evaluation-personal.component';
import { EvaluationManageComponent } from './pages/evaluation/evaluation-manage/evaluation-manage.component';
import { EvaluationTemplateComponent } from './pages/evaluation-template/evaluation-template.component';
import { WorkSpaceSettingComponent } from './pages/work-space-setting/work-space-setting.component';
import { TimeTrackerComponent } from './pages/time-tracker/time-tracker.component';
import { TimeSheetReportComponent } from './pages/time-sheet-report/time-sheet-report.component';

const routes: Routes = [
  {
    path: 'profile',
    component: ProfileComponent,
    canActivate: [AuthGuardService],
    data: {
      allowedRoles: [AppRoutingRole.ADMINISTRATOR, AppRoutingRole.USER],
    },
  },
  {
    path: 'criteria',
    component: CriteriasComponent,
    canActivate: [AuthGuardService],
    data: {
      allowedRoles: [AppRoutingRole.ADMINISTRATOR],
    },
  },
  {
    path: 'home',
    component: HomeComponent,
    canActivate: [AuthGuardService],
    data: {
      allowedRoles: [AppRoutingRole.ADMINISTRATOR, AppRoutingRole.USER],
    },
  },
  {
    path: 'login-form',
    component: LoginFormComponent,
  },
  {
    path: 'reset-password',
    component: ResetPasswordFormComponent,
    canActivate: [AuthGuardService],
  },
  {
    path: 'forgot-password',
    component: ForgotPasswordComponent,
    canActivate: [AuthGuardService],
  },
  {
    path: 'create-account',
    component: CreateAccountFormComponent,
    canActivate: [AuthGuardService],
  },
  {
    path: 'user',
    component: UserComponent,
    canActivate: [AuthGuardService],
    data: {
      allowedRoles: [AppRoutingRole.ADMINISTRATOR, AppRoutingRole.USER],
    },
  },
  {
    path: 'project',
    component: ProjectComponent,
    canActivate: [AuthGuardService],
    data: {
      allowedRoles: [AppRoutingRole.ADMINISTRATOR, AppRoutingRole.USER],
    },
  },
  {
    path: 'evaluation-template',
    component: EvaluationTemplateComponent,
    canActivate: [AuthGuardService],
    data: {
      allowedRoles: [AppRoutingRole.ADMINISTRATOR],
    },
  },
  {
    path: 'evaluation-personal',
    component: EvaluationPersonalComponent,
    canActivate: [AuthGuardService],
    data: {
      allowedRoles: [AppRoutingRole.ADMINISTRATOR, AppRoutingRole.USER],
    },
  },
  {
    path: 'evaluation-manage',
    component: EvaluationManageComponent,
    canActivate: [AuthGuardService],
    data: {
      allowedRoles: [AppRoutingRole.ADMINISTRATOR],
    },
  },
  {
    path: 'evaluation-leader',
    component: EvaluationLeaderComponent,
    canActivate: [AuthGuardService],
    data: {
      allowedRoles: [AppRoutingRole.ADMINISTRATOR, AppRoutingRole.USER],
    },
  },
  {
    path: 'timesheet/manage-workspace',
    component: WorkSpaceSettingComponent,
    canActivate: [AuthGuardService],
    data: {
      allowedRoles: [AppRoutingRole.ADMINISTRATOR],
    },
  },
  {
    path: 'timesheet/group',
    component: TimeSheetGroupComponent,
    canActivate: [AuthGuardService],
    data: {
      allowedRoles: [AppRoutingRole.ADMINISTRATOR],
    },
  },
  {
    path: 'timesheet/time-tracker',
    component: TimeTrackerComponent,
    canActivate: [AuthGuardService],
    data: {
      allowedRoles: [AppRoutingRole.ADMINISTRATOR, AppRoutingRole.USER],
    },
  },
  {
    path: 'timesheet/report',
    component: TimeSheetReportComponent,
    data: {
      allowedRoles: [AppRoutingRole.ADMINISTRATOR, AppRoutingRole.USER],
    }
  },
  {
    path: '**',
    redirectTo: 'login-form',
  },
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, { useHash: false }),
    DxDataGridModule,
    DxFormModule,
  ],
  providers: [AuthGuardService],
  exports: [RouterModule],
  declarations: [ProfileComponent],
})
export class AppRoutingModule {}
