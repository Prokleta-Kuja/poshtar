export default interface IListResponse{
    size: number;
    page: number;
    total: number;
    ascending: boolean;
    sortBy?: string;
}