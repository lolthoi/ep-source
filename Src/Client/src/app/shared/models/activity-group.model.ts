import { ActivityGroupUserModel } from './activity-group-user.model';
import { ActivityModel } from './activity.model';
export class ActivityGroupModel {
  id: string;
  projectId: number;
  name: string;
  description: string;
  activities: ActivityModel[];
  activityGroupUserModels: ActivityGroupUserModel[];
}
