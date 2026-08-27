import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WorkExperienceEntry, UpsertWorkExperienceRequest } from '../models/experience.models';

@Injectable({ providedIn: 'root' })
export class ExperienceService {
  private readonly apiUrl = `${environment.apiUrl}/experience`;

  constructor(private http: HttpClient) {}

  getMine(): Observable<WorkExperienceEntry[]> {
    return this.http.get<WorkExperienceEntry[]>(`${this.apiUrl}/me`);
  }

  getForUser(userId: number): Observable<WorkExperienceEntry[]> {
    return this.http.get<WorkExperienceEntry[]>(`${this.apiUrl}/${userId}`);
  }

  create(request: UpsertWorkExperienceRequest): Observable<WorkExperienceEntry> {
    return this.http.post<WorkExperienceEntry>(this.apiUrl, request);
  }

  update(id: number, request: UpsertWorkExperienceRequest): Observable<WorkExperienceEntry> {
    return this.http.put<WorkExperienceEntry>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  reorder(orderedIds: number[]): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/reorder`, { orderedIds });
  }
}