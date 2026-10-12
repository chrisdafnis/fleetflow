const { createApp } = Vue;

createApp({
    data() {
        return {
            email: "",
            password: "",
            loading: false,
            error: "",
            session: null,
            jobs: [],
            jobsLoading: false,
            jobsError: "",
            newReference: "",
            newDescription: "",
            creatingJob: false,
            createJobError: "",
            updatingJobId: null,
            jobActionError: ""
        };
    },

    methods: {
        async login() {
            if (this.loading) return;

            this.loading = true;
            this.error = "";

            try {
                const response = await fetch("/api/auth/login", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        email: this.email,
                        password: this.password
                    })
                });

                if (!response.ok) {
                    this.error = response.status === 401
                        ? "Invalid email or password."
                        : "Unable to sign in. Please try again.";
                    return;
                }

                this.session = await response.json();
                this.password = "";
                await this.loadJobs();
            } catch {
                this.error = "Could not connect to the server.";
            } finally {
                this.loading = false;
            }
        },

        logout() {
            this.session = null;
            this.password = "";
            this.error = "";
            this.jobs = [];
            this.newReference = "";
            this.newDescription = "";
            this.createJobError = "";
            this.jobActionError = "";
        },

        async loadJobs() {
            if (!this.session || this.jobsLoading) return;

            this.jobsLoading = true;
            this.jobsError = "";

            try {
                const response = await fetch(
                    `/api/tenants/${this.session.tenantId}/jobs`,
                    {
                        headers: {
                            Authorization: `Bearer ${this.session.accessToken}`
                        }
                    }
                );

                if (response.status === 401) {
                    this.logout();
                    this.error = "Your session has expired. Please sign in again.";
                    return;
                }

                if (!response.ok) {
                    this.jobsError = "Unable to load jobs. Please try again.";
                    return;
                }

                this.jobs = await response.json();
            } catch {
                this.jobsError = "Could not connect to the server.";
            } finally {
                this.jobsLoading = false;
            }
        },

        statusLabel(status) {
            const labels = {
                0: "Pending",
                1: "In progress",
                2: "Completed",
                3: "Cancelled"
            };

            return labels[status] ?? "Unknown";
        },

        async createJob() {
            if (!this.session || this.creatingJob) return;

            this.creatingJob = true;
            this.createJobError = "";

            try {
                const response = await fetch(
                    `/api/tenants/${this.session.tenantId}/jobs`,
                    {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json",
                            Authorization: `Bearer ${this.session.accessToken}`
                        },
                        body: JSON.stringify({
                            reference: this.newReference.trim(),
                            description: this.newDescription.trim()
                        })
                    }
                );

                if (response.status === 401) {
                    this.logout();
                    this.error = "Your session has expired. Please sign in again.";
                    return;
                }

                if (!response.ok) {
                    const problem = await response.json().catch(() => null);

                    if (response.status === 400 && problem?.errors) {
                        this.createJobError = Object.values(problem.errors)
                            .flat()
                            .join(" ");
                    } else if (response.status === 409) {
                        this.createJobError =
                            problem?.detail ?? "This job reference is already in use.";
                    } else {
                        this.createJobError =
                            "Unable to create the job. Please try again.";
                    }

                    return;
                }

                this.newReference = "";
                this.newDescription = "";

                await this.loadJobs();
            } catch {
                this.createJobError = "Could not connect to the server.";
            } finally {
                this.creatingJob = false;
            }
        },

        async changeJobStatus(job, action) {
            if (!this.session || this.updatingJobId) return;

            this.updatingJobId = job.id;
            this.jobActionError = "";

            try {
                const response = await fetch(
                    `/api/tenants/${this.session.tenantId}/jobs/${job.id}/${action}`,
                    {
                        method: "PATCH",
                        headers: {
                            Authorization: `Bearer ${this.session.accessToken}`
                        }
                    }
                );

                if (response.status === 401) {
                    this.logout();
                    this.error = "Your session has expired. Please sign in again.";
                    return;
                }

                if (!response.ok) {
                    this.jobActionError = response.status === 409
                        ? "This job's status has changed. Refresh the list and try again."
                        : "Unable to update the job. Please try again.";
                    return;
                }

                const updatedJob = await response.json();
                const index = this.jobs.findIndex(item => item.id === updatedJob.id);

                if (index !== -1) {
                    this.jobs.splice(index, 1, updatedJob);
                }
            } catch {
                this.jobActionError = "Could not connect to the server.";
            } finally {
                this.updatingJobId = null;
            }
        },
    }
}).mount("#app");