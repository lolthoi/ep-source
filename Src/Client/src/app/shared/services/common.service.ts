import { Injectable } from '@angular/core';
import notify from 'devextreme/ui/notify';
import { confirm } from 'devextreme/ui/dialog';
import { alert } from 'devextreme/ui/dialog';
import { title } from 'process';

@Injectable({
  providedIn: 'root',
})
export class CommonService {
  dialogControl = {
    notifiyManager: [],
    currentNotifyHeight: 0,
  };
  UI = {
    toastMessage(messages: string, type: string, time: any): void {
      const option = {
        position: {
          my: 'top right',
          at: 'top right',
          of: window,
          offset: '-5 10',
        },
        maxWidth: '400px',
        minHeight: '60px',
        message: messages,
      };
      return notify(option, type, time);
    },
    multipleNotify: (message: string, type: string, displayTime: any) => {
      const counter = this.dialogControl.notifiyManager.length + 1;
      notify({
        message,
        contentTemplate: (element: any) => {
          this.dialogControl.currentNotifyHeight +=
            element.lastChild.offsetHeight + 55;
          this.dialogControl.notifiyManager.push({
            counter,
            height: element.lastChild.offsetHeight + 55,
          });
        },
        type,
        show: {
          from: { opacity: 0.5, top: 100 },
          to: { opacity: 1, top: 10 },
          type: 'slideIn',
        },
        hide: {
          from: { opacity: 0.5, top: 10 },
          to: { opacity: 1, top: 10 },
          type: 'slideOut',
        },
        displayTime: displayTime || 4000,
        position: {
          my: 'top right',
          at: 'top right',
          of: window,
          offset: '5 +' + this.dialogControl.currentNotifyHeight,
        },
        maxWidth: '400px',
        minHeight: '70px',
        onHidden: (e) => {
          const thisNotifyHeight =
            this.dialogControl.notifiyManager[
              this.dialogControl.notifiyManager.length - 1
            ].height;
          this.dialogControl.notifiyManager.pop();
          this.dialogControl.currentNotifyHeight -= thisNotifyHeight;
        },
        collision: {
          x: 'flip',
          y: 'none',
        },
        closeOnClick: true,
      });
    },
    confirmBox: (message: string, title: string) => {
      return confirm('<i>' + message + '</i>', title);
    },
    alertBox: (message: string, title: string) => {
      return alert(message, title);
    },
  };

  getGuidEmpty = () => {
    return '00000000-0000-0000-0000-000000000000';
  };

  public convertSameDateTimeToDateUtc(date: Date) {
    return new Date(
      Date.UTC(
        date.getFullYear(),
        date.getMonth(),
        date.getDate(),
        date.getHours(),
        date.getMinutes(),
        date.getSeconds(),
        date.getMilliseconds()
      )
    );
  }

  constructor() {}
}
