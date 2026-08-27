import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EducationEditor } from './education-editor';

describe('EducationEditor', () => {
  let component: EducationEditor;
  let fixture: ComponentFixture<EducationEditor>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EducationEditor],
    }).compileComponents();

    fixture = TestBed.createComponent(EducationEditor);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
