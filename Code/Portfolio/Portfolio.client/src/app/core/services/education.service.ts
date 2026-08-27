import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EducationEntry, UpsertEducationRequest } from '../models/education.models';

@Injectable({ providedIn: 'root' })
export class EducationService {
  private readonly apiUrl = `${environment.apiUrl}/education`;

  constructor(private http: HttpClient) {}

  getMine(): Observable<EducationEntry[]> {
    return this.http.get<EducationEntry[]>(`${this.apiUrl}/me`);
  }

  getForUser(userId: number): Observable<EducationEntry[]> {
    return this.http.get<EducationEntry[]>(`${this.apiUrl}/${userId}`);
  }

  create(request: UpsertEducationRequest): Observable<EducationEntry> {
    return this.http.post<EducationEntry>(this.apiUrl, request);
  }

  update(id: number, request: UpsertEducationRequest): Observable<EducationEntry> {
    return this.http.put<EducationEntry>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  reorder(orderedIds: number[]): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/reorder`, { orderedIds });
  }
}