import { CriteriaTypeTemplateModel } from './criteria-type-template.model';

export class EvaluationTemplateModel {
  id: string;
  name: string;
  positionId: number;
  criteriaTypeTemplateModels: CriteriaTypeTemplateModel[];

  constructor(init?: Partial<EvaluationTemplateModel>) {
    Object.assign(this, init);
  }
}
