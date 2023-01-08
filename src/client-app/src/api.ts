export type Query = {
  skip?: number | null | undefined;
  take?: number | null | undefined;
  term?: string | null | undefined;
  sortBy?: string | null | undefined;
  descending?: boolean | undefined;
  includeDisabled?: boolean | undefined;
};

export class Api {
  private getOpt: RequestInit = {
    method: "GET",
    headers: { Accept: "application/json" },
  };

  private urlWithQueryString(url: string, params?: Query): string {
    if (!params) return url;

    const q = new URLSearchParams();
    if (params.skip) q.set("skip", params.skip.toString());
    if (params.take) q.set("take", params.take.toString());
    if (params.term) q.set("term", params.term);
    if (params.sortBy) q.set("sortBy", params.sortBy);
    if (params.descending)
      q.set("descending", params.descending ? "true" : "false");
    if (params.includeDisabled)
      q.set("includeDisabled", params.includeDisabled ? "true" : "false");

    let qs = q.toString();
    if (qs) return `${url}/?${qs}`;

    return url;
  }

  ///////////////////////////////////////////////////////

  domains(params?: Query): Promise<Domain[]> {
    const url = this.urlWithQueryString("/domains", params);
    return fetch(url, this.getOpt).then((r) => r.json() as Promise<Domain[]>);
  }

  domainById(id: number): Promise<Domain> {
    return fetch(`/domains/${id}`, this.getOpt).then(
      (r) => r.json() as Promise<Domain>
    );
  }

  domainAddresses(id: number, params?: Query): Promise<Address[]> {
    const url = this.urlWithQueryString(`/domains/${id}/addresses`, params);
    return fetch(url, this.getOpt).then((r) => r.json() as Promise<Address[]>);
  }
}

export type Address = {
  addressId?: number;
  domainId: number;
  pattern: string;
  description?: string;
  isStatic: boolean;
  disabled?: Date;
};

export type Domain = {
  domainId?: number;
  name: string;
  host: string;
  port: number;
  isSecure: boolean;
  username: string;
  password: string;
  disabled?: Date;
};

export type User = {
  userId?: number;
  name: string;
  description?: string;
  isMaster: boolean;
  quota?: number;
  disabled?: Date;
};

export type LogEntry = {
  logEntryId: number;
  timeStamp: Date;
  context: string;
  message: string;
  properties: {};
};
