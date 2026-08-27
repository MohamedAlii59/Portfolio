import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ExperienceEditor } from './experience-editor';

describe('ExperienceEditor', () => {
  let component: ExperienceEditor;
  let fixture: ComponentFixture<ExperienceEditor>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExperienceEditor],
    }).compileComponents();

    fixture = TestBed.createComponent(ExperienceEditor);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
