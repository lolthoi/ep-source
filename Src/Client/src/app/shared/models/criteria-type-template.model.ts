import { CriteriaTemplateModel } from './criteria-template.model';

export class CriteriaTypeTemplateModel {
  id: string;
  criteriaTypeStoreId: string;
  evaluationTemplateId: string;
  criteriaTemplateModels: CriteriaTemplateModel[];
  orderNo: number;

  constructor(init?: Partial<CriteriaTypeTemplateModel>) {
    Object.assign(this, init);
  }
}
