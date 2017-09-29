import { Component } from '@angular/core';
import { ReportsList } from './../';
import { NavController, NavParams } from 'ionic-angular';
import { Report, ReportsService } from '../../../shared/reports';

@Component({
    selector: 'bs-reports_confirm .bs-reports_confirm',
    templateUrl: 'reports-confirm.html'
})

export class ReportsConfirm {
    private report: Report;
    private today: number = Date.now();

    constructor(private navCtrl: NavController,
                private navParams: NavParams,
                private reportsService: ReportsService) {

        this.report = navParams.get('report');
    }

    private goToReports(): void {
        this.navCtrl.setRoot(ReportsList);
    }

    private undoSolved(): void {
        this.reportsService.markAsUnsolved(this.report).subscribe(() => {
            this.navCtrl.pop();
        });
    }
}
