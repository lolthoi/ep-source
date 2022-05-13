import { QuarterEvaluationModel } from "./quarter-evaluation.model";

export class DataSourcePersonalEvaluateModel {
    isAvaibleEvaluate: boolean;
    projectSource: ProjectDataSourceModel[];
    quarterSource: QuarterDataSourceModel[];
    yearSource: YearDataSourceModel[];
    dataSource: QuarterEvaluationModel[];
}

export class YearDataSourceModel {
    id: number;
    value: string;
}

export class QuarterDataSourceModel {
    id: number;
    yearId: number;
    value: string;
}

export class ProjectDataSourceModel {
    id: number;
    yearId: number;
    quarterId: number;
    value: string;
    projectId: number;
}