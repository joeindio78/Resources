import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Button,
  Card,
  CardContent,
  CardActions,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Grid,
  IconButton,
  MenuItem,
  Pagination,
  TextField,
  Typography,
  FormControl,
  InputLabel,
  Select,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
} from '@mui/icons-material';
import { getResources, deleteResource } from '@/services/api';

function ResourcesPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [sortBy, setSortBy] = useState('name');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [resourceToDelete, setResourceToDelete] = useState<number | null>(null);
  const pageSize = 6;

  const { data, isLoading } = useQuery({
    queryKey: ['resources', page, search, sortBy, sortDirection],
    queryFn: () =>
      getResources(page, pageSize, {
        name: search || undefined,
        sortBy,
        sortDirection,
      }),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteResource,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['resources'] });
      setDeleteDialogOpen(false);
      setResourceToDelete(null);
    },
  });

  const handlePageChange = (_: unknown, value: number) => {
    setPage(value);
  };

  const handleDeleteClick = (e: React.MouseEvent, resourceId: number) => {
    e.stopPropagation();
    setResourceToDelete(resourceId);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = () => {
    if (resourceToDelete) {
      deleteMutation.mutate(resourceToDelete);
    }
  };

  const handleEditClick = (e: React.MouseEvent, resourceId: number) => {
    e.stopPropagation();
    navigate(`/resources/${resourceId}/edit`);
  };

  if (isLoading) {
    return <Typography>Carregando...</Typography>;
  }

  return (
    <Box>
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between' }}>
        <Typography variant="h4">Recursos</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate('/resources/new')}
        >
          Adicionar Recurso
        </Button>
      </Box>

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12} md={6}>
          <TextField
            fullWidth
            label="Pesquisar por nome"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </Grid>
        <Grid item xs={12} md={3}>
          <FormControl fullWidth>
            <InputLabel>Ordenar por</InputLabel>
            <Select
              value={sortBy}
              label="Ordenar por"
              onChange={(e) => setSortBy(e.target.value)}
            >
              <MenuItem value="name">Nome</MenuItem>
              <MenuItem value="yearsOfExperience">Anos de Experiência</MenuItem>
              <MenuItem value="birthdate">Data de Nascimento</MenuItem>
            </Select>
          </FormControl>
        </Grid>
        <Grid item xs={12} md={3}>
          <FormControl fullWidth>
            <InputLabel>Direção</InputLabel>
            <Select
              value={sortDirection}
              label="Direção"
              onChange={(e) => setSortDirection(e.target.value as 'asc' | 'desc')}
            >
              <MenuItem value="asc">Ascendente</MenuItem>
              <MenuItem value="desc">Descendente</MenuItem>
            </Select>
          </FormControl>
        </Grid>
      </Grid>

      <Grid container spacing={2}>
        {data?.items.map((resource) => (
          <Grid item xs={12} sm={6} md={4} key={resource.id}>
            <Card>
              <CardContent
                sx={{ cursor: 'pointer' }}
                onClick={() => navigate(`/resources/${resource.id}`)}
              >
                <Typography variant="h6">{resource.name}</Typography>
                <Typography color="textSecondary" gutterBottom>
                  Experiência: {resource.yearsOfExperience} anos
                </Typography>
                {resource.birthDate && (
                  <Typography color="textSecondary" gutterBottom>
                    Data de Nascimento: {resource.birthDate}
                  </Typography>
                )}
                <Box sx={{ mt: 1 }}>
                  {resource.competencies.map((competency) => (
                    <Chip
                      key={competency.id}
                      label={competency.name}
                      size="small"
                      sx={{ mr: 0.5, mb: 0.5 }}
                    />
                  ))}
                </Box>
              </CardContent>
              <CardActions sx={{ justifyContent: 'flex-end' }}>
                <IconButton
                  size="small"
                  onClick={(e) => handleEditClick(e, resource.id)}
                  title="Editar"
                >
                  <EditIcon />
                </IconButton>
                <IconButton
                  size="small"
                  color="error"
                  onClick={(e) => handleDeleteClick(e, resource.id)}
                  title="Eliminar"
                >
                  <DeleteIcon />
                </IconButton>
              </CardActions>
            </Card>
          </Grid>
        ))}
      </Grid>

      {data && (
        <Box sx={{ mt: 3, display: 'flex', justifyContent: 'center' }}>
          <Pagination
            count={data.totalPages}
            page={page}
            onChange={handlePageChange}
            color="primary"
          />
        </Box>
      )}

      <Dialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
      >
        <DialogTitle>Confirmar Eliminação</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Tem certeza que deseja eliminar este recurso? Esta ação não pode ser desfeita.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancelar</Button>
          <Button
            onClick={handleDeleteConfirm}
            color="error"
            variant="contained"
            autoFocus
          >
            Eliminar
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default ResourcesPage; 