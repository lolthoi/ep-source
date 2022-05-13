export class ProjectModel {
    id: number;
    name: string;
    description: string;
    status: number;
    statusText: string;
    startDate: Date;
    endDate: Date;
  
    constructor(init?: Partial<ProjectModel>) {
      Object.assign(this, init);
    }
  }
  