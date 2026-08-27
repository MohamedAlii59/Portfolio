import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProfileResponse, UpdateProfileRequest } from '../models/profile.models';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly apiUrl = `${environment.apiUrl}/profile`;

  constructor(private http: HttpClient) {}

  getMyProfile(): Observable<ProfileResponse> {
    return this.http.get<ProfileResponse>(`${this.apiUrl}/me`);
  }

  getPublicProfile(slug: string): Observable<ProfileResponse> {
    return this.http.get<ProfileResponse>(`${this.apiUrl}/${slug}`);
  }

  updateProfile(request: UpdateProfileRequest): Observable<ProfileResponse> {
    return this.http.put<ProfileResponse>(`${this.apiUrl}/me`, request);
  }

  uploadPhoto(file: File): Observable<ProfileResponse> {
    const formData = new FormData();
    formData.append('File', file); // matches UploadPhotoRequestDto.File on the backend
    return this.http.post<ProfileResponse>(`${this.apiUrl}/me/photo`, formData);
  }

  uploadResume(file: File): Observable<ProfileResponse> {
    const formData = new FormData();
    formData.append('File', file); // matches UploadResumeRequestDto.File
    return this.http.post<ProfileResponse>(`${this.apiUrl}/me/resume`, formData);
  }

  deleteResume(): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/me/resume`);
  }
}