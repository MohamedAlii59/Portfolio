import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TechnologiesManager } from './technologies-manager';

describe('TechnologiesManager', () => {
  let component: TechnologiesManager;
  let fixture: ComponentFixture<TechnologiesManager>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TechnologiesManager],
    }).compileComponents();

    fixture = TestBed.createComponent(TechnologiesManager);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
