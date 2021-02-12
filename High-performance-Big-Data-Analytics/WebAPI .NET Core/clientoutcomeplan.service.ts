/*
Author          : System Analyst Oleg Gorlov
Description:	  : Service of service ClientOutcomePlanService
Copyright       : 
email           : oleg.gorlov@durham.ca
Date            : 19/09/2019
Release         : 1.0.0
Comment         : Implementation Angular 6 - 1.0 Released 19/09/2019
 */

import { Injectable, Component  } from "@angular/core";
import { throwError as observableThrowError, Observable } from "rxjs";
import { HttpClient } from '@angular/common/http';

import { map, catchError } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { TableModule } from 'primeng/table';

//Model
import { ISummary } from '../Models/summary.interface';
import { IClientOutcomePlan } from "../Models/clientoutcomeplan.interface";

@Injectable()
export class ClientOutcomePlanService {

  //--- From Controller WebAPI
 public _url_GetClientOutcomePlanByMemberId: string = '/api/PersonSearch/GetClientOutcomePlanByMemberId';

  public _url_GetAllClientOutcomePlan:  string = '/api/ClientOutcomePlan/GetAllClientOutcomePlan';
  public _url_GetClientOutcomePlanById: string = '/api/ClientOutcomePlan/GetClientOutcomePlanById';
  //public _url_SaveClientOutcomePlan: string = '/api/ClientOutcomePlan/SaveClientOutcomePlan';
  public _url_CreateClientOutcomePlan: string = '/api/ClientOutcomePlan/CreateClientOutcomePlan';

    public _url_SaveSummary: string = '/api/Summary/SaveSummary';
  

    constructor(private http: HttpClient) { }

  //--- Get All Users to fill UserForm
  getAllClientOutcomePlan() {
    return this.http.get(this._url_GetAllClientOutcomePlan)
      .pipe(map(res => <IClientOutcomePlan[]>res))
      .pipe(catchError(this.handleError));
  }

  //--- Get ClientOutcomePlan By MemberId
  //GetClientOutcomePlanByMemberId
  getClientOutcomePlanByMemberId(memberid: string): Observable<IClientOutcomePlan> {

    var getByIdUrl = this._url_GetClientOutcomePlanByMemberId + '/' + memberid

    return this.http.get(getByIdUrl)
      .pipe(map(res => <IClientOutcomePlan>res))
      .pipe(catchError(this.handleError));
  }



  //--- function Update ClientOutcomePlan for ClientOutcomePlanForm
  //search 
  search(memberid: string): Observable<IClientOutcomePlan> {

    var getByIdUrl = this._url_GetClientOutcomePlanByMemberId + '/' + memberid

    return this.http.get(getByIdUrl)

      .pipe(map(res => <IClientOutcomePlan>res))
      .pipe(catchError(this.handleError));
  }

  //--- SaveClientOutcomePlan
  saveClientOutcomePlan(client: IClientOutcomePlan): Observable<string> {
    let body = JSON.stringify(client);
    let headers = new Headers({ 'Content-Type': 'application/json' });
    //let options = new RequestOptions({ headers: headers });
      //return this.http.post(this._url_CreateClientOutcomePlan, body, options)
    return this.http.post(this._url_CreateClientOutcomePlan, body)
      .pipe(map(res => res.toString()))
      .pipe(catchError(this.handleError));
    }

    //--- function Update ISummary for ISummaryForm
    saveSummary(client: ISummary): Observable<string> {
        let body = JSON.stringify(client);
        let headers = new Headers({ 'Content-Type': 'application/json' });
        //let options = new RequestOptions({ headers: headers });

        //return this.http.post(this._url_SaveSummary, body, options)
        return this.http.post(this._url_SaveSummary, body)
            .pipe(map(res => res.toString()))
            .pipe(catchError(this.handleError));
    }


  //--- function Update Client for ClientForm
  //save(client: IClientOutcomePlan): Observable<string> {
  //  let body = JSON.stringify(client);
  //  let headers = new Headers({ 'Content-Type': 'application/json' });
  //  let options = new RequestOptions({ headers: headers });

  //  let getByIdUrl = 

  //  return this.http.post(this._url_SaveClient, body, options)
  //    .pipe(map(res => res.json().message))
  //    .pipe(catchError(this.handleError));
  //}

  //addCaseActivity(caseactivity: ICaseActivity) {
  //  return this.http.post("/api/caseactivity", caseactivity);
  //  }

  //editCaseActivity(caseactivity: ICaseActivity) {
  //  return this.http.put(`/api/caseactivity/${caseactivity.caseactivitieId}`, caseactivity);
  //  }

  //deleteCaseActivity(caseactivityId: number) {
  //    return this.http.delete(`/api/caseactivity/${productId}`);
  //  }

  private handleError(error: Response) {
    return observableThrowError(error || 'Opps!! Server error');
...
